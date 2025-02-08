using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{
    public class SwordsplosionRework : BaseMeleeItem
    {
        public override int ItemType => ModContent.ItemType<Swordsplosion>();
        public override int ProjectileType => ModContent.ProjectileType<SwordplosionSwordProj>();
        public override void Defaults(Item item)
        {

            item.damage = 50;
            item.useTime = 10;
            item.useAnimation = item.useTime;
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<Swordsplosion>())
            {
                var cplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
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

    }

    public class SwordplosionSwordProj : BaseMeleeSwing
    {
        public override int swingWidth => 270;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Item;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<Swordsplosion>()).Texture;

        public override int OffsetDistance => 60;

        public override int AfterImageLength => 5;

        public override float SwingFunction()
        {
            //return base.SwingFunction();
            return MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2);
        }
        public override void AdditionalAI()
        {
            if (timer % (swingTime / 6) == 0 && timer > 0 && timer < swingTime)
            {
                shootCheck(ModContent.ProjectileType<SwordsplosionBeam>(), 20, ai0: Main.rand.Next(0, 4));
            }
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

        public override string Texture => "DozeCalamityWeaponOverhaul/Reworks/Melee/Swordsplosion0";

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
            Texture2D texture = ModContent.Request<Texture2D>("DozeCalamityWeaponOverhaul/Reworks/Melee/Swordsplosion" + Projectile.ai[0]).Value;
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