using CalamityMod;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee.EarthLine
{
    public class GuardParry : ModBuff
    {
        public override string Texture => ModContent.GetModBuff(ModContent.BuffType<PhantomicShield>()).Texture;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
        }
    }
    public class MajesticGuardRework : BaseMeleeItem
    {
        public override int ItemType => ModContent.ItemType<MajesticGuard>();
        public override int ProjectileType => ModContent.ProjectileType<MajesticGuardSwordProj>();
        public override bool RClickAutoswing => true;

        /*public override void Defaults(Item item)
        {
            item.damage = 100;
        }*/

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ItemType)
            {
                if (player.altFunctionUse != 2)
                {

                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                    return false;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                    player.GetModPlayer<WeaponOverhaulPlayer>().ParryCooldown = 60 * 5;
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ItemType)
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ItemType)
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ProjectileType && proj.owner == player.whoAmI && proj.active)
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

    public class MajesticGuardSwordProj : BaseMeleeSwing
    {
        public override int swingWidth => 180 + 45;
        public bool old = false;
        private bool parry = false;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Texture;

        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<MajesticGuard>()).Item;

        List<float> oldProjectileRot = new List<float> { };
        List<Vector2> oldProjectilePos = new List<Vector2> { };
        public override void Spawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            if (player.HasBuff<GuardParry>())
            {
                Projectile.damage = (int)(Projectile.damage * 0.25f);
                parry = true;
                Projectile.scale *= 1.1f;
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

            if (Projectile.ai[0] == 0)  // lclick
            {
                if (!parry)
                {
                    var angle2 = MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, (float)timer / swingTime));
                    Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                }
                else
                {

                    var angle2 = (float)timer < swingTime / 2 ? MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, (float)timer * 2 / swingTime)) : -MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, ((float)timer * 2 - swingTime) / swingTime));
                    if (timer == swingTime / 2f)
                    {
                        Projectile.ResetLocalNPCHitImmunity();
                    }
                    Projectile.Center = armCenter - (angle * 70 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                }


                if ((float)timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }
            if (Projectile.ai[0] == 1) // rclick
            {
                Projectile.scale = 0.75f;
                Projectile.Center = player.Center + new Vector2(Projectile.spriteDirection * 10, -45) * player.gravDir;
                Projectile.rotation = MathHelper.ToRadians(Projectile.spriteDirection * -45);
                player.direction = Projectile.spriteDirection * (int)player.gravDir;
                Projectile.damage = 0;
                if (cplayer.ParryCooldown < 60 * 9) cplayer.ParryTime = 2;
                if ((float)timer > swingTime || player.HasBuff<GuardParry>())
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
                        Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, Main.player[Projectile.owner].gravDir == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);
                    }
                    else
                    {
                        Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, 1, Main.player[Projectile.owner].gravDir == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.FlipVertically, 0);
                    }
                }
            }
            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];
            var cplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
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
    }

}