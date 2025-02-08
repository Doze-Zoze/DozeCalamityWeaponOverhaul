using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Summon;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Summon;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Summon
{
    public class CosmicImmatRework : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.type == ModContent.ItemType<CosmicImmaterializer>())
            {
                entity.shoot = ModContent.ProjectileType<ExoticDagger>();
                entity.damage = 525;
            }
        }
    }

    public class DaggerTracker : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public List<int> daggerList = new List<int> { };
        public float daggerRot = 0;
        public int daggerNum = 0;
        public int daggerTimer = 0;
        public bool StopExtraUpdate = false;

        public override void PostAI(NPC npc)
        {
            for (var i = 0; i < daggerList.Count; i++)
            {
                if (!Main.projectile[daggerList[i]].active || Main.projectile[daggerList[i]].ModProjectile<ExoticDagger>().target != npc.whoAmI)
                {
                    daggerList.RemoveAt(i);
                    if (daggerNum >= daggerList.Count) daggerNum = 0;
                    i--;
                }
            }
            if (daggerList.Count > 0)
            {
                daggerTimer++;
            }
            else
            {
                daggerTimer = 0;
                daggerNum = 0;
                daggerRot = 0;
            }
            StopExtraUpdate = false;
        }

    }

    public class ExoticBuff : ModBuff
    {
        public override string Texture => ModContent.GetModBuff(ModContent.BuffType<DazzlingStabberBuff>()).Texture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ExoticDagger>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
            }
        }
    }

    public class ExoticDagger : ModProjectile
    {

        public override string Texture => ModContent.GetModProjectile(ModContent.ProjectileType<DazzlingStabber>()).Texture;
        public override void SetStaticDefaults()
        {

            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 100;
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ModContent.ProjectileType<DazzlingStabber>());
            Projectile.minionSlots = 1;
            Projectile.minion = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.frame = 0;
            Projectile.extraUpdates = 1;
        }

        public int target = -1;
        public bool attack = false;
        public int timer = 0;
        public Vector2 targetpos = Vector2.Zero;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (timer == 0)
            {
                player.AddBuff(ModContent.BuffType<ExoticBuff>(), 60);
            }
            if (player.HasBuff<ExoticBuff>())
            {
                Projectile.timeLeft = 10;
            }
            else
            {
                Projectile.Kill();
                return;
            }
            if (target != -1) if (Main.npc[target].CanBeChasedBy()) target = -1;
            Projectile.Minion_FindTargetInRange(4000, ref target, true);
            if (target != -1)
            {
                NPC tar = Main.npc[target];
                var GNPCtar = tar.GetGlobalNPC<DaggerTracker>();
                if (GNPCtar.StopExtraUpdate)
                {
                    Projectile.position -= Projectile.velocity;
                    return;
                }
                if (!tar.active)
                {
                    target = -1;
                }
                if (!GNPCtar.daggerList.Contains(Projectile.whoAmI))
                {
                    GNPCtar.daggerList.Add(Projectile.whoAmI);
                }
                for (var i = 0; i < GNPCtar.daggerList.Count; i++)
                {
                    if (GNPCtar.daggerList[i] == Projectile.whoAmI)
                    {
                        targetpos = tar.Center + new Vector2(0, -175).RotatedBy(GNPCtar.daggerRot + MathF.PI * 2 / GNPCtar.daggerList.Count * i + (GNPCtar.daggerNum > i ? MathF.PI * 2 / 8f * 5 : 0));
                        if (GNPCtar.daggerTimer >= 60 / GNPCtar.daggerList.Count && GNPCtar.daggerNum == i)
                        {
                            GNPCtar.daggerTimer = 0;
                            attack = true;
                            GNPCtar.daggerNum++;
                            if (GNPCtar.daggerNum >= GNPCtar.daggerList.Count)
                            {
                                GNPCtar.daggerNum = 0;
                                GNPCtar.daggerRot += MathF.PI * 2 / 8f * 5;
                                if (GNPCtar.daggerRot > MathF.PI * 2) GNPCtar.daggerRot -= MathF.PI * 2;
                            }
                        }
                    }
                }
                if (attack)
                {
                    if (Projectile.Center.Distance(tar.Center) >= 130)
                    {
                        if (Projectile.extraUpdates > 2)
                        {
                            Projectile.extraUpdates = 1;
                            //GNPCtar.StopExtraUpdate = true;
                            attack = false;
                            Projectile.velocity *= 3;
                        }
                        else
                        {

                            MoveTowards(tar.Center, 15, 2);
                        }
                    }
                    if (Projectile.Center.Distance(tar.Center) < 130)
                    {
                        if (Projectile.extraUpdates < 5) Projectile.ResetLocalNPCHitImmunity();
                        Projectile.extraUpdates = 15;
                    }

                }
                else
                {
                    MoveTowards(targetpos, 30, 10);
                    if (Projectile.Distance(targetpos) > 400)
                    {
                        MoveTowards(targetpos, 90, 10);
                    }
                }

                Projectile.rotation = Projectile.velocity.Length() > 5 ? Projectile.velocity.ToRotation() + MathF.PI / 2 : Projectile.DirectionTo(tar.Center).ToRotation() + MathF.PI / 2;
            }
            else
            {

                Projectile.extraUpdates = 2;
                var tar = Main.player[Projectile.owner];
                var GNPCtar = tar.GetModPlayer<WeaponOverhaulPlayer>();
                if (!GNPCtar.daggerList.Contains(Projectile.whoAmI))
                {
                    GNPCtar.daggerList.Add(Projectile.whoAmI);
                }
                for (var i = 0; i < GNPCtar.daggerList.Count; i++)
                {
                    if (GNPCtar.daggerList[i] == Projectile.whoAmI)
                    {
                        targetpos = tar.Center + new Vector2(0, -100).RotatedBy(MathHelper.ToRadians((float)Main.time * 2) + MathF.PI * 2 / GNPCtar.daggerList.Count * i + (GNPCtar.daggerNum > i ? MathF.PI * 2 / 8f * 5 : 0));
                    }
                }
                Projectile.rotation = Projectile.velocity.Length() > 3 ? Projectile.velocity.ToRotation() + MathF.PI / 2 : Projectile.DirectionTo(tar.Center).ToRotation() - MathF.PI / 2;
                MoveTowards(targetpos, 50, 15);
                if (Projectile.Distance(targetpos) > 3000)
                {
                    Projectile.Center = tar.Center;
                }
            }
            timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //based on Exobeam's trail
            Main.spriteBatch.EnterShaderRegion();
            Color mainColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.12f) % 1, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            Color secondaryColor = CalamityUtils.MulticolorLerp((Main.GlobalTimeWrappedHourly * 0.5f + Projectile.whoAmI * 0.12f + 0.2f) % 1, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            var TrailTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes", (AssetRequestMode)2);
            Vector2 trailOffset = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() + Projectile.Size * 0.5f;
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(TrailTexture);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();
            var positionsToUse = Projectile.oldPos.Take(60).ToArray();
            PrimitiveRenderer.RenderTrail(positionsToUse, new(trailWidth, trailColor, (_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladePierce"]), 25);
            Main.spriteBatch.ExitShaderRegion();
            lightColor = mainColor;
            return true;
        }
        public float trailWidth(float comp)
        {
            return MathHelper.Lerp(20, 0, comp);
        }

        public Color trailColor(float comp)
        {
            return new Color(0, 0, 0);
        }
        private void MoveTowards(Vector2 goal, float speed, float inertia)
        {
            //if (Projectile.Center.Distance(goal) <= 2) { Projectile.Center = goal; Projectile.velocity = Vector2.Zero; return; }
            Vector2 moveTo = (goal - Projectile.Center).SafeNormalize(Vector2.UnitY) * Math.Min(speed / 1.5f, Projectile.Distance(goal));
            Projectile.velocity = (Projectile.velocity * (inertia - 1) + moveTo) / inertia;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 60);
        }
    }
}
