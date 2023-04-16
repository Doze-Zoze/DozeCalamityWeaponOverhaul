using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using IL.Terraria.GameContent.NetModules;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace carnageRework.Items.Reworks.DevilsDevastationLine
{
    public class ExaltedOathbladeRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<ExaltedOathblade>())
            {
                item.damage = 120;
                item.useTime = 22;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<ExaltedOathbladeSwordProj>();
                item.autoReuse = true;
                item.scale = 2f;
            }
        }
        public override void HoldItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<ExaltedOathblade>())
            {
                var cplayer = player.GetModPlayer<CarnagePlayer>();
                cplayer.ExaltedChargeTime++;
            }

        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<ExaltedOathblade>())
            {
                if (player.altFunctionUse != 2)
                {

                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                    return false;
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                    return false;
                }
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<ExaltedOathblade>())
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<ExaltedOathblade>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<ExaltedOathbladeSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }

                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class ExaltedOathbladeSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 180;
        private int swingTime = 22 * 4;//ModContent.GetModItem(ModContent.ItemType<ExaltedOathblade>()).Item.useTime;
        public bool old = false;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<ExaltedOathblade>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 2400;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<ExaltedOathblade>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<ExaltedOathblade>()).Item.height;
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

            Projectile.extraUpdates = 3;
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
            if (cplayer.ExaltedChargeTime >= cplayer.ExaltedChargeMax)
            {
                ChargedAttack = true;
            }
            cplayer.ExaltedChargeTime = 0;
            player.direction = Projectile.spriteDirection;
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);

            if (Projectile.ai[0] == 0)  // lclick
            {

                if (ChargedAttack)
                {
                    Projectile.Center = armCenter - (angle * 50 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(player.direction * -MathHelper.ToRadians(timer * (270 / swingTime) - 270 / 2));
                    Projectile.rotation = angle.RotatedBy(player.direction * -MathHelper.ToRadians(timer * (270 / swingTime) - 270 / 2)).ToRotation() + adust;
                    shootCheck(Projectile.spriteDirection, 8, 1.15f, ModContent.ProjectileType<ExaltedOathbladeBeam>(), damagemod: 0.25f);
                }
                else
                {
                    Projectile.Center = armCenter - (angle * 50 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(player.direction * -MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2));
                    Projectile.rotation = angle.RotatedBy(player.direction * -MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2)).ToRotation() + adust;
                    shootCheck(Projectile.spriteDirection, 3, damagemod: 0.33f);
                }


                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 0;
                }
            }
            if (Projectile.ai[0] == 1) // rclick
            {

                if (ChargedAttack)
                {
                    if (!old)
                    {
                        cplayer.DashFrames = 35;
                        player.itemAnimation = 44;
                        player.itemTime = 44;
                        Projectile.timeLeft = 44 * 4;
                        timer = 0;
                        Projectile.damage *= 2;
                    }
                    if (Projectile.timeLeft > 80)
                    {
                        oldProjectileRot.Add(Projectile.rotation);
                        oldProjectilePos.Add(Projectile.Center);
                        if (oldProjectileRot.Count > 60)
                        {
                            oldProjectileRot.RemoveAt(0);
                            oldProjectilePos.RemoveAt(0);
                        }
                    }
                    else
                    {
                        oldProjectileRot.Add(Projectile.rotation);
                        oldProjectilePos.Add(Projectile.Center);
                        for (var i = 0; i < 2; i++)
                        {
                            if (oldProjectileRot.Count > 0)
                            {
                                oldProjectileRot.RemoveAt(0);
                                oldProjectilePos.RemoveAt(0);
                            }
                        }
                    }


                    var adjustment = MathHelper.ToRadians(180);
                    if (player.direction == 1)
                    {
                        adjustment = MathHelper.ToRadians(45 * 5);
                    }
                    angle = MathHelper.ToRadians(45 * 5.5f).ToRotationVector2();
                    angle.X *= player.direction;
                    Projectile.Center = armCenter - (angle * 50 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (360 / 70) - 360 / 2) * player.direction);
                    Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (360 / 70) - 360 / 2)).ToRotation() * player.direction + adjustment;
                    if (timer < 154)
                    {
                        player.fullRotation = player.Center.DirectionTo(Projectile.Center).ToRotation();
                        player.fullRotationOrigin = player.Center - player.position;
                    }
                    else
                    {
                        player.fullRotation = 0;
                    }
                }
                else
                {
                    if (!old)
                    {
                        cplayer.DashFrames = 10;
                        player.itemAnimation = 30;
                        player.itemTime = 30;
                        Projectile.timeLeft = 30 * 4;
                    }
                    angle = -player.Center.DirectionTo(Main.MouseWorld);
                    cplayer.DashType = 1;
                    timer = timer % swingTime;
                    Projectile.Center = armCenter - angle * 50 * (1 + (Projectile.scale - 1) * 0.75f);
                    Projectile.rotation = angle.ToRotation() + adust;

                }
            }
            timer++;
            old = true;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }
        private bool ChargedAttack = false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[0] == 1 && ChargedAttack)
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
        private int shootCount = 0;
        private void shootCheck(int negate = 1, int amount = 7, float velocitymod = 1, int type = 0, float damagemod = 1)
        {
            if (type == 0)
            {
                type = ModContent.ProjectileType<ForbiddenOathbladeBeam>();
            }
            amount += 1;
            if (timer % (swingTime / amount) == 0 && timer > 0 && timer < swingTime - swingTime / amount / 2)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * 20 * velocitymod, type, (int)(Projectile.damage * damagemod), Projectile.knockBack, Projectile.owner);
            }
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
            if (!ChargedAttack && Projectile.ai[0] == 1)
            {
                cplayer.DashFrames = 0;
                player.velocity *= -1;
                player.SetImmuneTimeForAllTypes(12);
            }
            if (ChargedAttack) target.AddBuff(BuffID.Daybreak, (int)(60 * 2.5));
            target.AddBuff(BuffID.ShadowFlame, (int)(60 * 2.5));
            target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), (int)(60 * 2.5));
        }

        public override void Kill(int timeLeft)
        {
            Main.player[Projectile.owner].fullRotation = 0;
        }
    }

    public class ExaltedOathbladeBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.width = 58;
            Projectile.height = 58;
            Projectile.friendly = true;
            Projectile.penetrate = 4;
            Projectile.localNPCHitCooldown = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override string Texture => ModContent.GetModProjectile(ModContent.ProjectileType<Oathblade>()).Texture;

        private Vector2 mouse = Vector2.Zero;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
        }
        public override void AI()
        {

            if (Projectile.timeLeft > 110)
            {
                Projectile.Opacity = (120 - Projectile.timeLeft) / 5f;
            }
            if (Projectile.timeLeft < 10)
            {
                Projectile.Opacity = Projectile.timeLeft / 10f;
            }


            if (Projectile.velocity.Length() < 1 && target == null)
            {
                NPC targetNPC = Projectile.FindTargetWithinRange(1600);
                if (targetNPC != null) { target = targetNPC.whoAmI; }

            }
            if (target == null)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation += MathHelper.ToRadians(45 / 2);
            }
            else
            {
                Projectile.rotation += MathHelper.ToRadians(45);
                if (Main.npc[(int)target].active == false)
                {
                    target = null;
                }
                else
                {
                    Projectile.velocity += Projectile.DirectionTo(Main.npc[(int)target].Center) * (0.25f + Projectile.velocity.Length() / 10f);
                    if (Projectile.velocity.Length() > 30)
                    {
                        Projectile.velocity /= Projectile.velocity.Length() / 30;
                    }
                    Projectile.timeLeft = 110;
                }
            }

        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        int? target = null;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 6;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, null, Color.White * col, oldProjectileRot[i], texture.Size() / 2, i / (float)max, SpriteEffects.None, 0);
                }
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Daybreak, 60);

        }
    }
}