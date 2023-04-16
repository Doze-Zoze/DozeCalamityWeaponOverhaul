using CalamityMod;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks
{
    public class SwordsplosionRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<Swordsplosion>())
            {
                item.damage = 50;
                item.useTime = 10;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.knockBack = 6;
                //item.value = 10000;
                //item.rare = 2;
                item.shoot = ModContent.ProjectileType<SwordplosionSwordProj>();
                item.autoReuse = true;
                item.scale = 2f;
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<Swordsplosion>())
            {
                var cplayer = player.GetModPlayer<CarnagePlayer>();
                if (cplayer.swingNum == 0)
                {
                    cplayer.swingNum++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                }
                else
                {
                    cplayer.swingNum--;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                }
                return false;
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<Swordsplosion>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<SwordplosionSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }
                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class SwordplosionSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 270;
        private int swingTime = ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Item.useTime * 2;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 240;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Item.height;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }
        List<float> oldProjectileRot = new List<float> { };
        List<Vector2> oldProjectilePos = new List<Vector2> { };
        List<Projectile> oldProjectile = new List<Projectile> { };
        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Zero;
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            if (angle.X < 0)
            {
                player.direction = 1;
            }
            else
            {
                player.direction = -1;
            }

            if (Projectile.ai[0] == 0)
            {

                Projectile.Center = player.Center - (angle * 57 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2)).ToRotation() + adust;
                shootCheck();


                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }


            if (Projectile.ai[0] == 1)
            {
                Projectile.spriteDirection = -1;
                Projectile.Center = player.Center - (angle * 57 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(-MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(-MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2)).ToRotation() + MathHelper.ToRadians(45 + -90);

                shootCheck(-1);
                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }

            }
            timer++;


            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (player.Center - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {
                var col = i / 5f * Projectile.Opacity;
                if (Projectile.spriteDirection == 1)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, SpriteEffects.None, 0);
                }
                else
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, SpriteEffects.FlipHorizontally, 0);
                }
            }
            oldProjectileRot.Add(Projectile.rotation);
            oldProjectilePos.Add(Projectile.Center);
            if (oldProjectileRot.Count > 5)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
            }
            return true;
        }

        private void shootCheck(int negate = 1)
        {
            if (timer % (swingTime / 6) == 0 && timer > 0 && timer < swingTime)
            {
                int rand = Main.rand.Next(0, 3);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * 20, ModContent.ProjectileType<SwordsplosionBeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Main.rand.Next(0, 4));
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.width * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
        }
    }

    public class SwordsplosionBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override string Texture => "carnageRework/Items/Reworks/Swordsplosion0";

        private Vector2 mouse = Vector2.Zero;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
        }

        private int bounceCount = 0;
        public override void AI()
        {

            if (Projectile.timeLeft > 110)
            {
                Projectile.Opacity = (120 - Projectile.timeLeft) / 5f;
            }
            else
            {
                Projectile.Opacity = -((15 - Projectile.velocity.Length()) / 30f - 1);
            }
            if (Projectile.timeLeft < 10)
            {
                Projectile.Opacity = Projectile.timeLeft / 10f;
            }
            Projectile.velocity *= 0.92f;
            /*if (Projectile.timeLeft == 210)
            {
                mouse = Main.MouseWorld + Vector2.Zero;
                Projectile.rotation = ( (Projectile.Center.DirectionTo(Main.MouseWorld)).ToRotation() + MathHelper.ToRadians(45));
                Projectile.velocity = (Projectile.rotation - MathHelper.ToRadians(45)).ToRotationVector2() * 10;
            }*/
            if (Projectile.velocity.Length() < 5 && bounceCount < 3)
            {
                Projectile.rotation = Projectile.Center.DirectionTo(Main.MouseWorld).ToRotation() + MathHelper.ToRadians(90);
                Projectile.velocity = (Projectile.rotation - MathHelper.ToRadians(90)).ToRotationVector2() * 50;
                Projectile.timeLeft = 40;
                bounceCount++;
                Projectile.ResetLocalNPCHitImmunity();
            }

        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("carnageRework/Items/Reworks/Swordsplosion" + Projectile.ai[0]).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 15;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, i / (float)max, SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
}