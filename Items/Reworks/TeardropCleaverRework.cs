using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing.Text;
using System.Linq;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks
{

    public class TeardropCleaverRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<TeardropCleaver>())
            {
                item.damage = 90;
                item.useTime = 23 * 2;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                //item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<TeardropCleaverSwordProj>();
                item.autoReuse = true;
                //item.scale = 2f;
                item.useTurn = false;
            }
        }
        public override void HoldItem(Item item, Player player)
        {
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<TeardropCleaver>())
            {
                if (player.altFunctionUse != 2)
                {

                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<TeardropCleaver>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<TeardropCleaverSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }
                if (player.GetModPlayer<CarnagePlayer>().ParryCooldown > 0 && player.altFunctionUse == 2) return false;

                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class TeardropCleaverSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 360;
        private int swingTime = 23 * 2;
        public bool old = false;
        private bool parry = false;
        private int projType;

        public PrimitiveTrail TrailDrawer;
        public static Asset<Texture2D> TrailTexture;

        public override string Texture => ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 3;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item.height;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item.DamageType;
            Projectile.tileCollide = false;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;

        }

        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            swingTime = (int)(swingTime / player.GetWeaponAttackSpeed(ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item));
            swingTime = swingTime < 1 ? 1 : swingTime;
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            angle = MathHelper.ToRadians(-45).ToRotationVector2();
            if ((player.Center - Main.MouseWorld).X < 0)
            {
                angle.X *= -1;
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.extraUpdates = 0;

            if (angle.X < 0)
            {
                player.direction = 1;
                Projectile.spriteDirection = 1;
            }
            else
            {
                player.direction = -1;
                Projectile.spriteDirection = -1;
            }
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            if (Projectile.spriteDirection == -1)
            {
                adust = MathHelper.ToRadians(-45);
            }
            var cplayer = player.GetModPlayer<CarnagePlayer>();
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);


            var angle2 = MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, MathHelper.SmoothStep(0, 1, timer / swingTime)));
            Projectile.Center = armCenter - (angle * 50 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
            Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;


            if (timer > swingTime)
            {
                Projectile.Kill();
                player.itemTime = 0;
            }

            timer++;
            old = true;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }

        public float TrailWidth(float completionRatio)
        {
            return MathHelper.SmoothStep(1, 0.1f, completionRatio) * 20;
        }

        public Color TrailColor(float completionRatio)
        {
            return Color.Lerp(Color.Blue, Color.Orange, completionRatio);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (TrailDrawer == null)
            {
                TrailDrawer = new PrimitiveTrail(TrailWidth, TrailColor, null, GameShaders.Misc["CalamityMod:ExobladePierce"]);
            }
            Vector2[] trailpos = new Vector2[] { };
            for (var i = 0; i < Projectile.oldPos.Length; i++)
            {
                trailpos.Append(Projectile.oldPos[i]);
            }
            Main.spriteBatch.EnterShaderRegion();
            if (TrailTexture == null)
            {
                TrailTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/BasicTrail", (AssetRequestMode)2);
            }
            GameShaders.Misc["CalamityMod:ExobladePierce"].SetShaderTexture(TrailTexture);

            GameShaders.Misc["CalamityMod:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladePierce"].UseColor(new Color(0, 10, 100));

            GameShaders.Misc["CalamityMod:ExobladePierce"].UseSecondaryColor(new Color(0, 100, 0));
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();
            GameShaders.Misc["CalamityMod:ExobladePierce"].Apply();
            TrailDrawer.Draw(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 30, Projectile.oldRot); ;
            Main.spriteBatch.ExitShaderRegion();

            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            return true;
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.height * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var player = Main.player[Projectile.owner];
            target.AddBuff(ModContent.BuffType<TemporalSadness>(), 120);
            if (target.type == NPCID.WallofFlesh || target.type == NPCID.WallofFleshEye)
            {
                for (var i = 0; i < 200; i++)
                {
                    var npc = Main.npc[i];
                    if (npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye)
                    {
                        npc.Center -= npc.velocity * 25;
                    }
                }
            }
            else
            {
                target.velocity += player.Center.DirectionTo(target.Center) * 10;
            }
        }

        public override void Kill(int timeLeft)
        {
        }
    }

}