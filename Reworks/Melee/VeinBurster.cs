using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{

    public class VeinBursterRework : BaseMeleeItem
    {
        public override int ItemType => ModContent.ItemType<VeinBurster>();
        public override int ProjectileType => ModContent.ProjectileType<VeinBursterSwordProj>();
        public override void Defaults(Item item)
        {
            item.damage = 32;
            item.useTime = 41;
            item.useAnimation = item.useTime;
        }

    }

    public class VeinBursterSwordProj : BaseMeleeSwing
    {
        public override int swingWidth => 200;
        public override string Texture => ModContent.GetModItem(BaseItem.type).Texture;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<VeinBurster>()).Item;
        public override int AfterImageLength => 0;
        public override int OffsetDistance => 30;

        public int swingTimeOffset = 10;

        public override void Spawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            swingTime = 41;
            Projectile.timeLeft = 75;
            modplayer.swingNum = 0;
        }

        public override void AdditionalAI()
        {
            if (timer - swingTimeOffset == (swingTime - swingTimeOffset) / 2)
            {
                var p = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.player[Projectile.owner].Center - angle * 20, -angle * 25, ModContent.ProjectileType<VeinBursterBall>(), (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner)];
            }
        }

        public override float SwingFunction()
        {
            if (timer < swingTimeOffset)
            {
                return MathHelper.ToRadians(-swingWidth / 2);
            }
            return MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, MathHelper.SmoothStep(0, MathHelper.SmoothStep(0, 2, (timer - swingTimeOffset) / ((float)swingTime - swingTimeOffset)), (timer - swingTimeOffset) / ((float)swingTime - swingTimeOffset))));
        }
    }

    public class VeinBursterBall : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/Melee/BloodBall";

        public int Target = -1;
        public bool stuck = false;
        public Vector2 offset = Vector2.Zero;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 4;
            Projectile.height = 20;
            Projectile.width = 20;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.RotatedBy(MathF.PI).ToRotation();
        }

        public override void AI()
        {
            if (!stuck)
            {
                if (Main.tile[Projectile.Center.ToTileCoordinates()].IsTileSolidGround() && Projectile.timeLeft <= 235)
                {
                    stuck = true;
                    //Projectile.position += Projectile.velocity;
                    Projectile.velocity = Vector2.Zero;
                    return;
                }
                Projectile.rotation += Projectile.velocity.Length() * 0.1f;
                if (Projectile.velocity.Y < 10 && !Projectile.wet) Projectile.velocity.Y += 0.25f;
                if (Projectile.wet && Projectile.velocity.Y > -7) Projectile.velocity.Y -= 0.7f;
                Projectile.velocity.X *= 0.98f;
            }
            if (Target != -1)
            {
                var target = Main.npc[Target];
                if (!target.active)
                {
                    Target = -1;
                    stuck = false;
                    return;
                }
                Projectile.position = target.position - offset;
                Projectile.velocity = target.velocity;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!stuck)
            {
                Projectile.position += Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
                stuck = true;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life <= 0) return;
            Target = target.whoAmI;
            stuck = true;
            offset = target.position - Projectile.position;
            Projectile.velocity = Vector2.Zero;
            target.AddBuff(ModContent.BuffType<BurningBlood>(), Projectile.timeLeft);
        }

        public override void OnKill(int timeLeft)
        {
            if (Target != -1) Main.npc[Target].SimpleStrikeNPC(Projectile.damage * 2, 0, Main.rand.Next(100) < Main.player[Projectile.owner].GetCritChance(DamageClass.Melee), damageType: DamageClass.Melee);
            SoundEngine.PlaySound(SoundID.NPCHit20, Projectile.position);
            for (int i = 0; i < 20; i++)
            {
                int blood = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1.5f);
                Main.dust[blood].velocity *= 1.4f;
            }
            for (int j = 0; j < 10; j++)
            {
                int bloody = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 2.5f);
                Main.dust[bloody].noGravity = true;
                Main.dust[bloody].velocity *= 5f;
                bloody = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1.5f);
                Main.dust[bloody].velocity *= 3f;
            }
        }


    }

}