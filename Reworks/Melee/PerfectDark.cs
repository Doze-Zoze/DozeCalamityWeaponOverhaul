using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{

    public class PerfectDarkRework : BaseMeleeItem
    {
        public override int ItemType => ModContent.ItemType<PerfectDark>();
        public override int ProjectileType => ModContent.ProjectileType<PerfectDarkSwordProj>();
        public override bool RClickAutoswing => true;
        public override void Defaults(Item item)
        {
            item.useTime = 20;
            item.damage = 45;
            item.useAnimation = item.useTime;
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<PerfectDark>()) return true;
            return base.AltFunctionUse(item, player);
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ItemType)
            {

                if (player.altFunctionUse == 2)
                {
                    type = ModContent.ProjectileType<ShadowKnife>();
                    int oldest = -1;
                    float oldestBright = 10;
                    int count = 0;
                    foreach (var p in Main.projectile)
                    {
                        if (p.type == type && p.owner == player.whoAmI && p.active)
                        {
                            count++;
                            var x = p.ModProjectile<ShadowKnife>().brightness;
                            if (x < oldestBright)
                            {
                                oldest = p.whoAmI;
                                oldestBright = x;
                            }
                        }
                    }
                    if (count > 2 && oldest != -1)
                    {
                        Main.projectile[oldest].Kill();
                    }
                    Projectile.NewProjectile(source, position, velocity * 2, type, (int)(damage * 0.33f), knockback, player.whoAmI);
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

        public override void HoldItem(Item item, Player player)
        {
            if (item.type == ItemType)
            {
                player.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity += 0.1f;
                if (player.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity > 1) player.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity = 1;
            }
            base.HoldItem(item, player);
        }
    }

    public class PerfectDarkSwordProj : BaseMeleeSwing
    {
        public override int swingWidth => 200;
        public override string Texture => ModContent.GetModItem(BaseItem.type).Texture;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<PerfectDark>()).Item;
        public override int AfterImageLength => 5;
        public override int OffsetDistance => 40;

        public override bool useMeleeSpeed => true;

        public int swingTimeOffset = 0;

        public override void Spawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            swingTime = 25;
            Projectile.timeLeft = 75;
            if (modplayer.swingNum++ >= 1) modplayer.swingNum = 0;
        }

        public override void AdditionalAI()
        {
            if (timer - swingTimeOffset == (swingTime - swingTimeOffset) / 2)
            {
                var p = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.player[Projectile.owner].Center - angle, -angle * 25, ProjectileID.NightBeam, (int)(Projectile.damage * 0.75f), Projectile.knockBack, Projectile.owner)];
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

        public override float trailWidth(float comp)
        {
            return base.trailWidth(comp) * 1.5f;
        }

        public override Color trailColor(float comp)
        {
            return Color.Black;
        }

    }

    public class OldTNESwordBeamBuffEdit : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ProjectileID.NightBeam)
            {
                target.AddBuff(BuffID.CursedInferno, 120);
            }
        }
    }

    public class ShadowKnife : ModProjectile
    {

        public int Target = -1;
        public bool stuck = false;
        public Vector2 offset = Vector2.Zero;
        public float brightness = 10;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 16;
            Projectile.height = 20;
            Projectile.width = 20;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.light = 1.5f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.RotatedBy(MathF.PI).ToRotation();
        }

        public override void AI()
        {
            if (brightness > 1) brightness *= 0.999f;
            if (brightness <= 1) Projectile.Kill(); else Projectile.timeLeft = 10;
            if (!stuck)
            {
                /*if (Main.tile[Projectile.Center.ToTileCoordinates()].IsTileSolidGround() && Projectile.timeLeft <= 235)
                {
                    stuck = true;
                    //Projectile.position += Projectile.velocity;
                    Projectile.velocity = Vector2.Zero;
                    return;
                }*/
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

                target.AddBuff(ModContent.BuffType<BrainRot>(), 30);
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
            target.AddBuff(ModContent.BuffType<BrainRot>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            if (Target != -1) Main.npc[Target].SimpleStrikeNPC((int)(Projectile.damage * (10f / brightness / 10 * 4f)), 0, Main.rand.Next(100) < Main.player[Projectile.owner].GetCritChance(DamageClass.Melee), damageType: DamageClass.Melee);
            SoundEngine.PlaySound(SoundID.Item103, Projectile.position);
            //same logic as the dust from the base rot ball in cal
            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 1.4f;
            }
            for (int j = 0; j < 10; j++)
            {
                int dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 2.5f);
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].velocity *= 5f;
                dust2 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust2].velocity *= 3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = ModContent.Request<Texture2D>(Texture).Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), Color.Purple, Projectile.rotation + MathF.PI / 2 * 2.5f, texture.Size() / 2, 0.75f, SpriteEffects.None, 0);

            return false;
        }


    }

    public class DarknessDmgMultiplier : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            var player = Main.player[projectile.owner];
            var mplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            float dmgMod = 1;
            if (mplayer.darkIntensity > 0 && modifiers.DamageType.CountsAsClass(DamageClass.Melee))
            {
                float closestDistance = 100;
                float closestBrightness = 10;
                float currentBrightness = 1; // 1 is full dark
                foreach (var proj in Main.projectile)
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<ShadowKnife>())
                    {

                        var x = npc.Center.Distance(proj.Center) / 16f;
                        if (x <= closestDistance)
                        {
                            closestBrightness = (proj.ModProjectile as ShadowKnife).brightness;
                            closestDistance = x;
                            currentBrightness = MathF.Min(currentBrightness, MathHelper.Lerp(0, 1, MathF.Pow(closestDistance / closestBrightness, 2f) / Math.Clamp(closestBrightness / MathF.Max(closestBrightness, 5), 0, 1)));
                        }


                    }
                }
                foreach (var pl in Main.player)
                {
                    if (pl.active)
                    {
                        var x = npc.Center.Distance(player.Center) / 16f;

                        if (x <= closestDistance)
                        {
                            closestBrightness = 12f;
                            closestDistance = x;
                            currentBrightness = MathF.Min(currentBrightness, MathHelper.Lerp(0, 1, MathF.Pow(closestDistance / closestBrightness, 2f) / Math.Clamp(closestBrightness / MathF.Max(closestBrightness, 5), 0, 1)));
                        }
                    }
                }
                modifiers.SourceDamage *= 1 + mplayer.darkIntensity * currentBrightness * 1f;

            }
        }
    }

    public class Darkness : ModSystem
    {
        public override void Load()
        {
            On_Main.DrawPlayerChatBubbles += On_Main_DrawPlayerChatBubbles;
            On_Main.DrawInterface_14_EntityHealthBars += DeleteHealthbars;
        }

        private void DeleteHealthbars(On_Main.orig_DrawInterface_14_EntityHealthBars orig, Main self)
        {

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.LocalPlayer.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity > 0.5f)
                {
                    return;
                }
            }
            orig(self);
        }

        private void On_Main_DrawPlayerChatBubbles(On_Main.orig_DrawPlayerChatBubbles orig, Main self)
        {

            orig(self); //we draw right after player chat bubbles because that's the final item before combat text is drawn, which needs to be visible so you can tell if an enemy takes damage.

            Main.spriteBatch.End();// here, spritebatch hadn't yet ended
            int resolution = 1;
            var pl = Main.LocalPlayer.Center.ToTileCoordinates();
            var pOff = Main.LocalPlayer.Center - pl.ToWorldCoordinates();
            pOff.Y *= Main.LocalPlayer.gravDir;
            var lightList = new Dictionary<Point16, float>();
            foreach (var proj in Main.projectile)
            {
                if (proj.type == ModContent.ProjectileType<ShadowKnife>() && proj.active)
                {
                    if (!lightList.ContainsKey(proj.Center.ToTileCoordinates16())) lightList.Add(proj.Center.ToTileCoordinates16(), proj.ModProjectile<ShadowKnife>().brightness);
                }
                if (proj.type == ProjectileID.NightBeam && proj.active)
                {
                    if (!lightList.ContainsKey(proj.Center.ToTileCoordinates16())) lightList.Add(proj.Center.ToTileCoordinates16(), 2.5f);
                }
            }
            if (Main.netMode != NetmodeID.Server) // This all needs to happen client-side!
            {
                GraphicsDevice _graphicsDevice = Main.graphics.GraphicsDevice;
                Texture2D _texture;
                Texture2D screenCover;
                int width = 121 * resolution;
                int height = 81 * resolution;
                int halfwidth = (width + 1 - resolution) / 2;
                int halfheight = (height + 1 - resolution) / 2;
                float[] darknessData = new float[width * height];
                Array.Fill(darknessData, -1);
                bool[] isDark = new bool[width * height];
                if (Main.LocalPlayer.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity > 0)
                {
                    Color[] textureData = new Color[width * height];
                    Color[] darknessRender = new Color[width * height];
                    for (int i = 0; i < width * height; i++)
                    {

                        var x = i % width - halfwidth;
                        var y = (int)Math.Floor((double)(i / width)) - halfheight;
                        var r = 0;
                        var b = lightList.ContainsKey(new Point16(x / resolution + pl.X, y / resolution + pl.Y)) ? lightList[new Point16(x / resolution + pl.X, y / resolution + pl.Y)] : 0;
                        if (x == 0 && y == 0) b = 12;
                        b *= resolution;
                        if (darknessData[i] == -1) darknessData[i] = 0;
                        if (r == 0)
                        {
                            isDark[i] = true;
                        }
                        /*if (b == 3)
                        {
                            for (int i2 = 0; i2 < 7 * 7; i2++)
                            {
                                var dx = (i2 % 7 - 3);
                                var dy = ((int)Math.Floor((double)(i2 / 7)) - 3);
                                if (dx + x > -60 && dx + x <= 60 && dy + y > -40 && dy + y <= 40)
                                {
                                    darknessData[dx + x + 60 + (dy + y + 40) * 121] = Math.Min(darknessData[dx + x + 60 + (dy + y + 40) * 121] == -1 ? 255 : darknessData[dx + x + 60 + (dy + y + 40) * 121], bakedDarkness3[i2]);
                                }
                            }
                        }
                        else*/
                        if (b > 0)
                        {
                            darknessData[i] = 0;
                            for (var dx = -(int)Math.Floor(b); dx <= Math.Ceiling(b); dx++)
                            {
                                for (var dy = -(int)Math.Floor(b); dy <= Math.Ceiling(b); dy++)
                                {
                                    if (dx + x > -halfwidth && dx + x <= halfwidth && dy + y > -halfheight && dy + y <= halfheight)
                                    {

                                        //darknessData[dx + x + 60 + (dy + y + 40) * 121] = Math.Min(darknessData[dx + x + 60 + (dy + y + 40) * 121] == -1 ? 255 : darknessData[dx + x + 60 + (dy + y + 40) * 121], (int)(255 * (new Vector2(dx, dy).Distance(Vector2.Zero) / b)));
                                        var existingData = darknessData[dx + x + halfwidth + (dy + y + halfheight) * width] == -1 ? 0 : darknessData[dx + x + halfwidth + (dy + y + halfheight) * width];
                                        //var newData = 1-((new Vector2(dx, dy).Distance(Vector2.Zero) / b)); // flat fistanc calc
                                        var newData = 1 - new Vector2(dx, dy).Distance(Vector2.Zero) / b;
                                        darknessData[dx + x + halfwidth + (dy + y + halfheight) * width] = Math.Max(newData, existingData);
                                    }
                                }
                            }
                        }
                    }
                    for (int i = 0; i < width * height; i++)
                    {
                        textureData[i] = new Color(0, 0, 0, 1 - darknessData[i]);
                    }
                    _texture = new Texture2D(_graphicsDevice, width, height);
                    _texture.SetData(textureData);
                    Main.spriteBatch.Begin();
                    var tilecord = pl.ToWorldCoordinates() - pOff;
                    Main.spriteBatch.Draw(texture: _texture, position: tilecord - Main.screenPosition, _texture.Frame(), Color.White * Main.LocalPlayer.GetModPlayer<WeaponOverhaulPlayer>().darkIntensity, 0, new Vector2(60.5f * resolution, 40.5f * resolution), 16 / resolution * Main.GameViewMatrix.Zoom.X, Main.LocalPlayer.gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 1);

                    Main.spriteBatch.End();
                    if (Main.mapStyle == 1) Main.mapStyle++;

                }
            }
            Main.spriteBatch.Begin();
        }
    }

}