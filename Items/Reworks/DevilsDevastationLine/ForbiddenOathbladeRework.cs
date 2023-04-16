using CalamityMod;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
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

namespace carnageRework.Items.Reworks.DevilsDevastationLine
{
    public class ForbiddenOathbladeRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<ForbiddenOathblade>())
            {
                //item.damage = 50;
                item.useTime = 54;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<ForbiddenOathbladeSwordProj>();
                item.autoReuse = true;
                item.scale = 2f;
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<ForbiddenOathblade>())
            {
                if (player.altFunctionUse != 2)
                {
                    if (player.direction == 1)
                    {
                        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                    }
                    else
                    {
                        Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                    }
                    return false;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 2);
                    return false;
                }
            }
            /*if (item.type == ItemID.CopperShortsword)
            {
                var cplayer = player.GetModPlayer<CarnagePlayer>();
                cplayer.DashFrames = 60;
                player.itemAnimation = 60;
                player.itemTime = 60;
                item.noUseGraphic = false;
                item.useStyle = ItemUseStyleID.HoldUp;
            }*/
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<ForbiddenOathblade>())
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<ForbiddenOathblade>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<ForbiddenOathbladeSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }

                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class ForbiddenOathbladeSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 180;
        private int swingTime = ModContent.GetModItem(ModContent.ItemType<ForbiddenOathblade>()).Item.useTime;
        private int chargeTime = 0;
        private int chargeMax = 60 * 4;
        public bool channel = true;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<ForbiddenOathblade>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 240;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<ForbiddenOathblade>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<ForbiddenOathblade>()).Item.height;
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
            angle = MathHelper.ToRadians(45 * 5.5f).ToRotationVector2();
            angle.X *= player.direction;
            Projectile.velocity = Vector2.Zero;

            if (Projectile.ai[0] == 2)
            {
                Projectile.extraUpdates = 3;
            }
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            if (angle.X < 0)
            {
                player.direction = 1;
            }
            else
            {
                player.direction = -1;
            }
            if (Projectile.ai[0] == 1)
            {
                Projectile.spriteDirection = -1;
            }
            else
            {
                Projectile.spriteDirection = 1;
            }

            float adust = MathHelper.ToRadians(90 + (45 + 90) * Projectile.spriteDirection);
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);
            if (Projectile.ai[0] == 0)
            {
                var angle2 = MathHelper.ToRadians(-MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer / swingTime));
                Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                shootCheck();


                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }


            if (Projectile.ai[0] == 1)
            {
                var angle2 = MathHelper.ToRadians(-MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer / swingTime));
                Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;

                shootCheck(-1);
                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }

            }


            if (Projectile.ai[0] == 2)
            {
                if (Main.mouseRight && channel)
                {
                    chargeTime++;
                    Projectile.timeLeft = 2;
                }
                else
                {
                    if (channel)
                    {
                        channel = false;
                        if (chargeTime >= chargeMax)
                        {
                            var cplayer = player.GetModPlayer<CarnagePlayer>();
                            cplayer.DashFrames = 35;
                            Projectile.damage *= 2;
                            player.itemAnimation = 44;
                            player.itemTime = 44;
                            Projectile.timeLeft = 44 * 4;
                            timer = 0;
                        }
                    }
                }
                if (Projectile.timeLeft > 80 && !channel)
                {
                    oldProjectileRot.Add(Projectile.rotation);
                    oldProjectilePos.Add(Projectile.Center);
                    if (oldProjectileRot.Count > 60)
                    {
                        oldProjectileRot.RemoveAt(0);
                        oldProjectilePos.RemoveAt(0);
                    }
                }
                else if (!channel)
                {
                    oldProjectileRot.Add(Projectile.rotation);
                    oldProjectilePos.Add(Projectile.Center);
                    for (var i = 0; i < 2; i++)
                    {
                        if (oldProjectileRot.Count > 0)
                        {
                            oldProjectileRot.RemoveAt(0);
                            oldProjectilePos.RemoveAt(0);
                        }
                    }
                }

                if (chargeTime == chargeMax)
                {
                    SoundEngine.PlaySound(SoundID.Item103);
                }
                if (chargeTime >= chargeMax)
                {
                    Dust.NewDust(Projectile.Center - new Vector2(5, -10).RotatedBy(Projectile.rotation + MathHelper.ToRadians(45)), 5, 5, DustID.Shadowflame);
                }

                if (chargeTime >= chargeMax && !channel)
                {
                    var adjustment = MathHelper.ToRadians(90);
                    if (player.direction == 1)
                    {
                        adjustment = MathHelper.ToRadians(45 * 5);
                    }
                    Projectile.Center = armCenter - (angle * 50 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (360 / 70) - 360 / 2) * player.direction);
                    Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (360 / 70) - 360 / 2)).ToRotation() * player.direction + adjustment;
                    if (timer < 154)
                    {
                        player.fullRotation = player.Center.DirectionTo(Projectile.Center).ToRotation();
                        player.fullRotationOrigin = player.Center - player.position;
                    }
                    else
                    {
                        player.fullRotation = 0;
                    }
                }
                else
                {
                    Projectile.Center = armCenter + new Vector2(10 * player.direction, -50);
                    Projectile.rotation = MathHelper.ToRadians(45 - 90);
                }
            }
            timer++;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 2 && !channel)
            {
                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                for (int i = 0; i < oldProjectileRot.Count; i++)
                {
                    var col = i / (60 / 4f) * Projectile.Opacity * 0.1f;
                    if (Projectile.spriteDirection == 1)
                    {
                        Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, SpriteEffects.None, 0);
                    }
                    else
                    {
                        Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, SpriteEffects.FlipHorizontally, 0);
                    }
                }
            }
            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            return true;
        }

        private void shootCheck(int negate = 1)
        {
            if (timer % (swingTime / 6) == 0 && timer > 0 && timer < swingTime)
            {
                int rand = Main.rand.Next(0, 3);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * 20, ModContent.ProjectileType<ForbiddenOathbladeBeam>(), (int)(Projectile.damage * 0.33f), Projectile.knockBack, Projectile.owner);
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

        public override void Kill(int timeLeft)
        {
            Main.player[Projectile.owner].fullRotation = 0;
        }
    }

    public class ForbiddenOathbladeBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.localNPCHitCooldown = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override string Texture => ModContent.GetModProjectile(ModContent.ProjectileType<ForbiddenOathbladeProjectile>()).Texture;

        private Vector2 mouse = Vector2.Zero;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
        }
        public override void AI()
        {

            if (Projectile.timeLeft > 110)
            {
                Projectile.Opacity = (120 - Projectile.timeLeft) / 5f * 0.75f;
            }
            else if (Projectile.timeLeft < 10)
            {
                Projectile.Opacity = Projectile.timeLeft / 10f * 0.75f;
            }
            else
            {
                Projectile.Opacity = 0.75f;
            }


            if (Projectile.velocity.Length() < 1 && target == null)
            {
                NPC targetNPC = Projectile.FindTargetWithinRange(1600);
                if (targetNPC != null) { target = targetNPC.whoAmI; }

            }
            if (target == null)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation += MathHelper.ToRadians(45 / 2);
            }
            else
            {
                Projectile.rotation += MathHelper.ToRadians(45);
                if (Main.npc[(int)target].active == false)
                {
                    target = null;
                }
                else
                {
                    Projectile.velocity += Projectile.DirectionTo(Main.npc[(int)target].Center) * (0.25f + Projectile.velocity.Length() / 10f);
                    if (Projectile.velocity.Length() > 30)
                    {
                        Projectile.velocity /= Projectile.velocity.Length() / 30;
                    }
                    Projectile.timeLeft = 110;
                }
            }

        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        int? target = null;
        public override bool PreDraw(ref Color lightColor)
        {
            //Texture2D texture = ModContent.Request<Texture2D>(this.Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            /*int max = 4;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
            }
            oldProjectileRot.Add(Projectile.rotation);
            oldProjectilePos.Add(Projectile.Center);
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {
                
                var col = ((float)Math.Pow(i/(float)max,2))*Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, i/(float)max, SpriteEffects.None, 0);
                }
            }*/
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return true;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.ShadowFlame, 30);
        }
    }
}