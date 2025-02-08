using CalamityMod;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee
{

    public class TeardropCleaverRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<TeardropCleaver>())
            {
                item.damage = 75;
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
                if (player.GetModPlayer<WeaponOverhaulPlayer>().ParryCooldown > 0 && player.altFunctionUse == 2) return false;

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

        public PrimitiveType TrailDrawer;
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

            Projectile.light = 0.25f;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;

        }

        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            swingTime = (int)(swingTime / player.GetWeaponAttackSpeed(ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item));
            swingTime = swingTime < 1 ? 1 : swingTime;
            angle = MathHelper.ToRadians(-45).ToRotationVector2();
            angle.Y *= player.gravDir;
            if ((player.Center - Main.MouseWorld).X < 0)
            {
                angle.X *= -1;
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.extraUpdates = 0;

            if (angle.X < 0)
            {
                player.direction = 1;
                Projectile.spriteDirection = 1 * (int)player.gravDir;
            }
            else
            {
                player.direction = -1;
                Projectile.spriteDirection = -1 * (int)player.gravDir;
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
            var cplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
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
            var armDir = armCenter - Projectile.Center;
            armDir.Y *= player.gravDir;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armDir.ToRotation() + MathHelper.ToRadians(90));


        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (timer > swingTime - 2) return true;
            Main.spriteBatch.EnterShaderRegion();
            if (TrailTexture == null)
            {
                TrailTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/VoronoiShapes", (AssetRequestMode)2);
            }
            Vector2 trailOffset = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2() + Projectile.Size * 0.5f;
            GameShaders.Misc["CalamityMod:ExobladeSlash"].SetShaderTexture(TrailTexture);

            //GameShaders.Misc["CalamityMod:ExobladeSlash"].UseImage2("Images/Extra_189");
            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseColor(new Color(118, 95, 77));

            GameShaders.Misc["CalamityMod:ExobladeSlash"].UseSecondaryColor(new Color(117, 151, 164));
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(Color.Blue.ToVector3());

            GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["flipped"].SetValue(Projectile.spriteDirection == 1);
            GameShaders.Misc["CalamityMod:ExobladeSlash"].Apply();

            var positionsToUse = Projectile.oldPos.Take(25).ToArray();
            for (var i = 0; i < positionsToUse.Length; i++)
            {
                if (i >= timer) continue;
                positionsToUse[i] += (Projectile.oldRot[i] - MathHelper.PiOver4 * (Projectile.spriteDirection == -1 ? 3 : 1)).ToRotationVector2() * 25;
            }
            PrimitiveRenderer.RenderTrail(positionsToUse, new(trailWidth, trailColor, (_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]), 25);
            Main.spriteBatch.ExitShaderRegion();

            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            return true;
        }

        public float trailWidth(float comp)
        {
            return MathHelper.Lerp(10, 0, comp);
        }

        public Color trailColor(float comp)
        {
            return new Color(0, 10, 100);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.height * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
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