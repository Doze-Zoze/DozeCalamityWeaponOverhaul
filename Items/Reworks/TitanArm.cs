using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
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
    public class TitanArmRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<TitanArm>())
            {
                //Item.damage = 50;
                //Item.DamageType = DamageClass.Melee;
                item.width = 46;
                item.height = 58;
                item.useTime = 20;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.knockBack = 6;
                item.value = 10000;
                item.rare = 2;
                item.shoot = ModContent.ProjectileType<TitanArmProj>();
                item.UseSound = SoundID.Item1;
                item.autoReuse = true;
                item.scale = 2f;
                item.channel = true;
                item.useTurn = true;
            }
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<TitanArm>())
            {
                return true;
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<TitanArm>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<TitanArmProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }
                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }
    }

    public class TitanArmProj : ModProjectile
    {
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<TitanArm>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 240;
            Projectile.width = 46;
            Projectile.height = 58;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;


        }

        private int damage;

        private List<List<float>> swingAnimFrames = new List<List<float>>();

        private void AddFrameModify(float X = 0, float Y = 0, float Rotation = 0, float Scale = 0)
        {
            swingAnimFrames.Add(new List<float>() { swingAnimFrames[swingAnimFrames.Count - 1][0] + X, swingAnimFrames[swingAnimFrames.Count - 1][1] + Y, swingAnimFrames[swingAnimFrames.Count - 1][2] + Rotation, swingAnimFrames[swingAnimFrames.Count - 1][3] + Scale });
        }
        public override void OnSpawn(IEntitySource source)
        {
            damage = Projectile.damage;
            Projectile.damage = 0;
            Projectile.scale = 0.75f;
            Projectile.knockBack = 90;
            swingAnimFrames.Add(new List<float>() { 38f, -8f, 230f, 0.75f });
            AddFrameModify(-2, Rotation: 2);

            AddFrameModify(-1, Rotation: 2);
            AddFrameModify(-1, Rotation: 4);
            AddFrameModify(-1, Rotation: 5);
            AddFrameModify(-2, Rotation: -5);
            AddFrameModify(-4, -3, Rotation: -10);
            AddFrameModify(-5, -15, Rotation: -25);
            AddFrameModify(-15, -13, Rotation: -30);
            AddFrameModify(-12, 0, Rotation: -40);
            AddFrameModify(-17, 0, Rotation: -30, 0.1f);
            AddFrameModify(-16, 15, Rotation: -30, 0.1f);
            AddFrameModify(-8, 20, Rotation: -20, 0.1f);
            AddFrameModify(-4, 20, Rotation: -20, 0.1f);
            AddFrameModify(10, 16, Rotation: -20);
            AddFrameModify(5, 8, Rotation: -20);
            AddFrameModify(8, 10, Rotation: -10);
            AddFrameModify(15, 10, Rotation: -15);
            AddFrameModify(5, -3, Rotation: -5, -0.1f);
            AddFrameModify(3, -3, Rotation: -5, -0.1f);
            AddFrameModify(3, -2, Rotation: -5, -0.1f);

        }
        public int chargeFrames;
        public bool fullCharge = false;
        public int hitEnemy = 1;
        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            int armangle = 20;
            int rotation = 180 + 50;
            int offsetX = 40;
            int timeLeftMax = 30;
            Vector2 sparkOffset = new Vector2(25, -10);
            if (player.channel)
            {
                if (player.velocity.Y == 0) player.velocity *= 0.9f;
                if (player.velocity.X != 0 && player.velocity.Y == 0) chargeFrames++;
                if (chargeFrames >= 59)
                {
                    chargeFrames = 59;
                    if (!fullCharge)
                    {
                        fullCharge = true;
                        SoundEngine.PlaySound(SoundID.Item10);
                    }
                }
                Projectile.timeLeft = timeLeftMax;
                if (player.direction == 1)
                {
                    Projectile.Center = player.Center - new Vector2(offsetX, -8);
                    Projectile.rotation = MathHelper.ToRadians(rotation);
                    Projectile.spriteDirection = 1;
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, +MathHelper.ToRadians(armangle));
                    if (player.velocity.X != 0 && player.velocity.Y == 0) Dust.NewDust(Projectile.Center - sparkOffset, 0, 0, DustID.MartianSaucerSpark);
                }
                else
                {
                    Projectile.Center = player.Center - new Vector2(-offsetX, -8);
                    Projectile.rotation = -MathHelper.ToRadians(rotation);
                    Projectile.spriteDirection = -1;
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, +MathHelper.ToRadians(-armangle));
                    sparkOffset.X *= -1;
                    sparkOffset.X += 5;

                    if (player.velocity.X != 0 && player.velocity.Y == 0) Dust.NewDust(Projectile.Center - sparkOffset, 0, 0, DustID.MartianSaucerSpark);
                }
            }
            else
            {
                var anim = timeLeftMax - Projectile.timeLeft;
                player.direction = Projectile.spriteDirection;
                if (anim == 1 && player.velocity.Y == 0)
                {
                    player.velocity += new Vector2(10f * player.direction, 0);
                }
                if (hitEnemy == 1 && anim < 20)
                {
                    player.velocity += new Vector2(0.40f * player.direction, 0);
                }
                if (anim > 17 && player.velocity.X > player.wingAccRunSpeed)
                {
                    player.velocity *= 0.95f;

                }
                Projectile.damage = damage * (1 + chargeFrames);
                if (swingAnimFrames.Count > anim)
                {

                    Projectile.Center = player.Center - new Vector2(swingAnimFrames[anim][0] * Projectile.spriteDirection, swingAnimFrames[anim][1]);
                    Projectile.rotation = MathHelper.ToRadians(swingAnimFrames[anim][2]) * Projectile.spriteDirection;
                    Projectile.scale = swingAnimFrames[anim][3];

                    if (player.velocity.X != 0) Dust.NewDust(Projectile.Center - sparkOffset, 0, 0, DustID.MartianSaucerSpark);
                }
                else
                {
                    anim = swingAnimFrames.Count - 1;
                    Projectile.Center = player.Center - new Vector2(swingAnimFrames[anim][0] * Projectile.spriteDirection, swingAnimFrames[anim][1]);
                    Projectile.rotation = MathHelper.ToRadians(swingAnimFrames[anim][2]) * Projectile.spriteDirection;
                    Projectile.scale = swingAnimFrames[anim][3];

                    if (player.velocity.X != 0) Dust.NewDust(Projectile.Center - sparkOffset, 0, 0, DustID.MartianSaucerSpark);
                }
            }
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (Projectile.Center + new Vector2(-24 * Projectile.scale * Projectile.spriteDirection, 30 * Projectile.scale).RotatedBy(Projectile.rotation) - player.Center).ToRotation() + MathHelper.ToRadians(-90 - 10 * Projectile.spriteDirection));
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Vector2 offset = new Vector2(5, -5) * Projectile.scale;
            int size = (int)(60 * Projectile.scale);

            if (Projectile.spriteDirection == -1)
            {
                offset.X *= -1;

            }
            var center = Projectile.Center + offset.RotatedBy(Projectile.rotation);
            hitbox = new Rectangle() { Location = (center - new Vector2(size / 2, size / 2)).ToPoint(), Width = size, Height = size };
            if (Main.player[Projectile.owner].channel == false && Projectile.timeLeft > 17)
            {
                hitbox = new Rectangle() { Location = (Main.player[Projectile.owner].position + new Vector2(40 * Projectile.spriteDirection, -20)).ToPoint(), Width = Main.player[Projectile.owner].width + 40, Height = Main.player[Projectile.owner].height + 40 };
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            Main.player[Projectile.owner].velocity.X *= -1;
            hitEnemy *= -1;
            Main.player[Projectile.owner].SetImmuneTimeForAllTypes(12);
        }
    }
}