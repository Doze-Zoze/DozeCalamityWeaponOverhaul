using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Typeless;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using rail;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks.EarthLine
{
    public class GuardParry : ModBuff
    {
        public override string Texture => ModContent.GetModBuff(ModContent.BuffType<ElysianGuard>()).Texture;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Guardian Parry");
            Description.SetDefault("Basically, you're melting");
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
        }
    }
    public class MajesticGuardRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<MajesticGuard>())
            {
                //item.damage = 666;
                //item.useTime = 19;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                //item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<MajesticGuardSwordProj>();
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
            if (item.type == ModContent.ItemType<MajesticGuard>())
            {
                if (player.altFunctionUse != 2)
                {

                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                    return false;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                    player.GetModPlayer<CarnagePlayer>().ParryCooldown = 60 * 5;
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<MajesticGuard>())
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<MajesticGuard>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<MajesticGuardSwordProj>() && proj.owner == player.whoAmI && proj.active)
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

    public class MajesticGuardSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 180 + 45;
        private int swingTime = ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Item.useTime;
        public bool old = false;
        private bool parry = false;
        private int projType;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Texture;

        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 3;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Item.height;
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
        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Zero;
            Projectile.extraUpdates = 0;

            if (player.HasBuff<GuardParry>())
            {
                Projectile.damage = (int)(Projectile.damage * 0.25f);
                parry = true;
            }
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

            if (Projectile.ai[0] == 0)  // lclick
            {
                if (!parry)
                {
                    var angle2 = MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer / swingTime));
                    Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                }
                else
                {

                    var angle2 = timer < swingTime / 2 ? MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer * 2 / swingTime)) : -MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, (timer * 2 - swingTime) / swingTime));
                    if (timer == swingTime / 2)
                    {
                        Projectile.ResetLocalNPCHitImmunity();
                    }
                    Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                }


                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }
            if (Projectile.ai[0] == 1) // rclick
            {
                Projectile.scale = 0.75f;
                Projectile.Center = player.Center + new Vector2(Projectile.spriteDirection * 10, -45);
                Projectile.rotation = MathHelper.ToRadians(Projectile.spriteDirection * -45);
                player.direction = Projectile.spriteDirection;
                Projectile.damage = 0;
                cplayer.ParryTime = 2;
                if (timer > swingTime || player.HasBuff<GuardParry>())
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }
            timer++;
            old = true;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));
            if (Projectile.ai[0] == 1)
            {
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center - new Vector2(0, 30)).ToRotation() + MathHelper.ToRadians(90));

            }


        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 1)
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

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.width * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var player = Main.player[Projectile.owner];
            var cplayer = player.GetModPlayer<CarnagePlayer>();
            if (target.Calamity().miscDefenseLoss < target.defense)
            {
                target.Calamity().miscDefenseLoss++;

            }
            if (target.Calamity().miscDefenseLoss < target.defense)
            {
                CombatText.NewText(player.Hitbox, Color.SkyBlue, target.defense - target.Calamity().miscDefenseLoss, target.boss, !target.boss);
            }
            if (!player.moonLeech && target.Calamity().miscDefenseLoss >= target.defense && target.canGhostHeal)
            {
                player.statLife += parry ? 1 : 3;


                player.HealEffect(parry ? 1 : 3);
            }
        }

        public override void Kill(int timeLeft)
        {
        }
    }

}