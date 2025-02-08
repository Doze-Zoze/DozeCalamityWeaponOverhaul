using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{
    public class ViolenceRework : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<Violence>())
            {
                item.useTime = item.useAnimation = 60;
                item.noMelee = true;
                item.shoot = ModContent.ProjectileType<ViolenceProjectile>();
                item.shootSpeed = 25f;
                item.channel = false;
                item.damage = 8888;
            }
        }

        public override bool CanUseItem(Item item, Player player)
        {
            return true;
        }

    }

    public class ViolenceProjectile : ModProjectile
    {
        private int hitstop = 0;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<Violence>()).Texture;

        public override void SetStaticDefaults()
        {
            //CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            //CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            var v = ModContent.GetModProjectile(ModContent.ProjectileType<ViolenceThrownProjectile>());
            Projectile.timeLeft = 60;
            Projectile.width = v.Projectile.width;
            Projectile.height = v.Projectile.height;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 4;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;

        }

        private Vector2 NormalVelocity = Vector2.Zero;
        private NPC target;
        private Vector2 targetoffset = Vector2.Zero;
        private int[] offsetadjust = new int[]
        {
            ModContent.NPCType<AresBody>(),
            ModContent.NPCType<AresLaserCannon>(),
            ModContent.NPCType<AresTeslaCannon>(),
            ModContent.NPCType<AresGaussNuke>(),
            ModContent.NPCType<AresPlasmaFlamethrower>(),
        };

        public override void OnSpawn(IEntitySource source)
        {
            NormalVelocity = Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathF.PI / 4;
        }
        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();

            if (hitstop > 0)
            {
                hitstop--;
                Projectile.timeLeft++;
                Projectile.velocity = NormalVelocity * 0.01f + (offsetadjust.Contains(target.type) ? target.position - targetoffset : Vector2.Zero);
                targetoffset = target.position;

                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.65f);
                Vector2 bloodSpawnPosition = target.Center + Main.rand.NextVector2Circular(target.width, target.height) * 0.04f;
                Vector2 splatterDirection = Projectile.velocity.SafeNormalize(Vector2.Zero) * -1;

                // Emit blood if the target is organic.
                if (target.Organic())
                {
                    SoundEngine.PlaySound(SoundID.NPCHit18, Projectile.Center);
                    for (int i = 0; i < 1; i++)
                    {
                        int bloodLifetime = Main.rand.Next(22, 36);
                        float bloodScale = Main.rand.NextFloat(0.6f, 0.8f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat());
                        bloodColor = Color.Lerp(bloodColor, new Color(51, 22, 94), Main.rand.NextFloat(0.65f));

                        if (Main.rand.NextBool(20))
                            bloodScale *= 2f;

                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.81f) * Main.rand.NextFloat(11f, 23f);
                        bloodVelocity.Y -= 12f;
                        BloodParticle blood = new BloodParticle(bloodSpawnPosition, bloodVelocity, bloodLifetime, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                    for (int i = 0; i < 1; i++)
                    {
                        float bloodScale = Main.rand.NextFloat(0.2f, 0.33f);
                        Color bloodColor = Color.Lerp(Color.Red, Color.DarkRed, Main.rand.NextFloat(0.5f, 1f));
                        Vector2 bloodVelocity = splatterDirection.RotatedByRandom(0.9f) * Main.rand.NextFloat(9f, 14.5f);
                        BloodParticle2 blood = new BloodParticle2(bloodSpawnPosition, bloodVelocity, 20, bloodScale, bloodColor);
                        GeneralParticleHandler.SpawnParticle(blood);
                    }
                }

                // Emit sparks if the target is not organic.
                else
                {
                    for (int i = 0; i < 1; i++)
                    {
                        int sparkLifetime = Main.rand.Next(22, 36);
                        float sparkScale = Main.rand.NextFloat(0.8f, 1f) + 1 * 0.85f;
                        Color sparkColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.7f));
                        sparkColor = Color.Lerp(sparkColor, Color.Orange, Main.rand.NextFloat());

                        if (Main.rand.NextBool(10))
                            sparkScale *= 2f;

                        Vector2 sparkVelocity = splatterDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(12f, 25f);
                        sparkVelocity.Y -= 6f;
                        SparkParticle spark = new SparkParticle(impactPoint, sparkVelocity, true, sparkLifetime, sparkScale, sparkColor);
                        GeneralParticleHandler.SpawnParticle(spark);
                    }
                }


            }
            else
            {
                Projectile.velocity = NormalVelocity;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();
            var TrailTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes", (AssetRequestMode)2);
            Vector2 trailOffset = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() + Projectile.Size * 0.5f;
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(TrailTexture);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(new Color(100, 0, 0));
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(new Color(100, 0, 0));
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();

            var positionsToUse = Projectile.oldPos.Take(60).ToArray();
            PrimitiveRenderer.RenderTrail(positionsToUse, new(trailWidth, trailColor, (_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), 25);
            Main.spriteBatch.ExitShaderRegion();
            return true;
        }

        public float trailWidth(float comp)
        {
            return MathHelper.Lerp(20, 0, comp);
        }

        public Color trailColor(float comp)
        {
            return new Color(100, 0, 0);
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(45 * Projectile.scale);
            hitbox.Width = (int)(45 * Projectile.scale);
            hitbox.Location = (center + NormalVelocity.SafeNormalize(Vector2.Zero) * 70 - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            this.target = target;
            targetoffset = target.position;
            float x = Utils.GetLerpValue(1000f, 10000f, damageDone, true);
            int x1 = (int)(20 * x * (Projectile.extraUpdates + 1));
            if (damageDone > 5) hitstop = x1;
            target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 240);
            //Violence's normal hit code

            if (Main.netMode != NetmodeID.Server)
            {
                // Play a splatter and impact sound.
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

                float damageInterpolant = Utils.GetLerpValue(950f, 2000f, hit.Damage, true);
                float impactAngularVelocity = MathHelper.Lerp(0.08f, 0.2f, damageInterpolant);
                float impactParticleScale = MathHelper.Lerp(0.6f, 1f, damageInterpolant);
                impactAngularVelocity *= Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(0.75f, 1.25f);

                Color impactColor = Color.Lerp(Color.Silver, Color.Gold, Main.rand.NextFloat(0.5f));
                Vector2 impactPoint = Vector2.Lerp(Projectile.Center, target.Center, 0.65f);


                // And create an impact point particle.
                ImpactParticle impactParticle = new ImpactParticle(impactPoint, impactAngularVelocity, 20, impactParticleScale, impactColor);
                GeneralParticleHandler.SpawnParticle(impactParticle);
            }

        }
    }
}