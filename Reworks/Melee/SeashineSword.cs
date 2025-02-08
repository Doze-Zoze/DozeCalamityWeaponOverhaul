using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{

    public class SeashineRework : BaseMeleeItem
    {
        public override int ProjectileType => ModContent.ProjectileType<SeashineProj>();
        public override int ItemType => ModContent.ItemType<SeashineSword>();
        public override void Defaults(Item item)
        {
            item.useTime = 20;
            item.useAnimation = item.useTime;
        }
    }

    public class SeashineProj : BaseMeleeSwing
    {
        public override int swingWidth => 200;
        public override string Texture => ModContent.GetModItem(BaseItem.type).Texture;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<SeashineSword>()).Item;
        public override int AfterImageLength => 0;
        public override int OffsetDistance => 30;

        //public override bool useMeleeSpeed => false;

        public override void Spawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            swingTime = 30;
            Projectile.timeLeft = 75;
            if (++modplayer.swingNum > 1) modplayer.swingNum = 0;
            Projectile.light = 1f;
        }

        public override void AdditionalAI()
        {
            if (timer == swingTime / 2)
            {
                var p = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.player[Projectile.owner].Center - angle * 40, -angle, ModContent.ProjectileType<WaterWave>(), (int)(Projectile.damage * 1.2f), Projectile.knockBack, Projectile.owner)];
            }
        }

        public override float SwingFunction()
        {
            return MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, MathHelper.SmoothStep(0, MathHelper.SmoothStep(0, 2, timer / (float)swingTime), timer / (float)swingTime)));
        }
    }

    public class WaterWave : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Melee/MendedBiomeBlade_GestureForTheDrownedAquaWave";
        public override void SetDefaults()
        {
            Projectile.timeLeft = 120 * 5;
            Projectile.height = 10;
            Projectile.width = 10;
            DrawOriginOffsetY = -132 / 2 + (int)Projectile.Size.Y / 2;
            DrawOffsetX = -30 / 2 + (int)Projectile.Size.Y / 2;
            Projectile.ignoreWater = true;
            Projectile.friendly = true;
            Projectile.extraUpdates = 4;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;

            Projectile.light = 0.33f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.RotatedBy(MathF.PI).ToRotation();
            Projectile.velocity.Normalize();
        }

        public override void AI()
        {
            Projectile.velocity *= 1.015f;
            Projectile.velocity = Projectile.velocity.ClampMagnitude(0, 30);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.damage = (int)(Projectile.damage * 0.8f);
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float coneLength = 90f * Projectile.scale;
            float maximumAngle = 0.8f;
            float coneRotation = Projectile.velocity.ToRotation();
            if (targetHitbox.IntersectsConeSlowMoreAccurate(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 60f, coneLength, coneRotation, maximumAngle))
            {
                return true;
            }
            return false;
        }
    }

}