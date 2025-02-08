using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee.DevilsDevastationLine
{
    public class DevilsDevastationRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<DevilsDevastation>())
            {
                item.damage = 666;
                item.useTime = 19;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<DevilsDevastationSwordProj>();
                item.autoReuse = true;
                PrefixLegacy.ItemSets.SwordsHammersAxesPicks[item.type] = true;
            }
        }
        public override void HoldItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<DevilsDevastation>())
            {
                var cplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
                cplayer.ExaltedChargeTime++;
            }
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<DevilsDevastation>())
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
            if (item.type == ModContent.ItemType<DevilsDevastation>())
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<DevilsDevastation>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<DevilsDevastationSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }

                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class DevilsDevastationSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int swingWidth = 180;
        private int swingTime = 23 * 4;// ModContent.GetModItem(ModContent.ItemType<DevilsDevastation>()).Item.useTime*4;
        public bool old = false;
        private bool ChargedAttack = false;
        private int projType;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<DevilsDevastation>()).Texture;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 2400;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<DevilsDevastation>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<DevilsDevastation>()).Item.height;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }
        List<float> oldProjectileRot = new List<float> { };
        List<Vector2> oldProjectilePos = new List<Vector2> { };
        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Zero;
            Projectile.extraUpdates = 3;
            if (modplayer.ExaltedChargeTime < modplayer.ExaltedChargeMax && Projectile.ai[0] == 0)
            {

                Projectile.extraUpdates = 3;
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
            if (modplayer.swingNum % 2 == 1 && Projectile.ai[0] == 0)
            {
                Projectile.spriteDirection *= -1;
            }
            if (modplayer.swingNum == 0)
            {
                modplayer.swingNum++;
            }
            else if (modplayer.swingNum == 1)
            {
                modplayer.swingNum = 0;
            }
            if (false)
            {
                var speed = Main.player[Projectile.owner].GetAttackSpeed<MeleeDamageClass>();
                if (speed > 3f)
                    speed = 3f;

                if (speed != 0f)
                    speed = 1f / speed;

                swingTime = (int)(swingTime * speed);
                if (swingTime < 1)
                {
                    swingTime = 1;
                }
            }
            if (true)
            {
                if (player.meleeScaleGlove) Projectile.scale *= 1.1f;
                Projectile.scale *= player.HeldItem.scale;
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
            if (cplayer.ExaltedChargeTime >= cplayer.ExaltedChargeMax)
            {
                ChargedAttack = true;
            }
            cplayer.ExaltedChargeTime = 0;
            if (cplayer.swingNum % 2 == 0 && Projectile.ai[0] == 0)
            {
                player.direction = -Projectile.spriteDirection;
            }
            else
            {
                player.direction = Projectile.spriteDirection;
            }
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);

            if (Projectile.ai[0] == 0)  // lclick
            {

                if (ChargedAttack)
                {
                    var angle2 = MathHelper.ToRadians(-MathHelper.SmoothStep(-270 / 2, 270 / 2, timer / swingTime));
                    Projectile.Center = armCenter - (angle * 80 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                    shootCheck(Projectile.spriteDirection, 9, 1f, ModContent.ProjectileType<ExaltedOathbladeBeam>(), damagemod: 0.25f);
                    shootAboveBelow();
                }
                else
                {

                    var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
                    var angle2 = MathHelper.ToRadians(-MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer / swingTime));
                    Projectile.Center = armCenter - (angle * 80 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
                    Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;
                    if (timer == swingTime / 2)
                    {


                        projType = ModContent.ProjectileType<SightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.15f, type: projType, damagemod: 0.2f);

                        projType = ModContent.ProjectileType<MightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.15f, type: projType, damagemod: 0.2f);

                        projType = ModContent.ProjectileType<FrightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.15f, type: projType, damagemod: 0.2f);
                        projType = ModContent.ProjectileType<NightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.15f, type: projType, damagemod: 0.2f);

                        projType = ModContent.ProjectileType<LightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.15f, type: projType, damagemod: 0.2f);

                        projType = ModContent.ProjectileType<FlightSoul>();
                        shootCheck(Projectile.spriteDirection, 0, 1.45f, type: projType, damagemod: 0.2f);

                    }
                    else
                    {
                        shootCheck(Projectile.spriteDirection, 2, damagemod: 0.33f);
                    }

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
                    if (Projectile.timeLeft % (12 * 4) == 0)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ForbiddenOathbladeBeam>(), (int)(Projectile.damage * 0.5f * 0.33f), Projectile.knockBack, Projectile.owner);
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

            var armDir = armCenter - Projectile.Center;
            armDir.Y *= player.gravDir;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armDir.ToRotation() + MathHelper.ToRadians(90));


        }
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
            if (amount == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * 20 * velocitymod, type, (int)(Projectile.damage * damagemod), Projectile.knockBack, Projectile.owner);
            }
            amount += 1;
            if (timer % (swingTime / amount) == 0 && timer > 0 && timer < swingTime - swingTime / amount / 2)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * 20 * velocitymod, type, (int)(Projectile.damage * damagemod), Projectile.knockBack, Projectile.owner);
            }
        }

        private void shootAboveBelow()
        {
            var amount = 5;
            if (timer % (swingTime / amount) == 0 && timer > 0 && timer < swingTime - swingTime / amount / 2)
            {
                var starpos = new Vector2(Main.MouseWorld.X + Main.rand.Next(-400, 400 + 1), Main.MouseWorld.Y - Main.screenHeight - 160);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), starpos, starpos.DirectionTo(Main.MouseWorld) * Main.rand.Next(75, 125) / 100 * 15f, ModContent.ProjectileType<DevilsDevastationBeam>(), (int)(Projectile.damage * 1.15f), Projectile.knockBack, Projectile.owner, 0);
                starpos = new Vector2(Main.MouseWorld.X + Main.rand.Next(-400, 400 + 1), Main.MouseWorld.Y + Main.screenHeight + 160);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), starpos, starpos.DirectionTo(Main.MouseWorld) * Main.rand.Next(75, 125) / 100 * 15f, ModContent.ProjectileType<DevilsDevastationBeam>(), (int)(Projectile.damage * 1.15f), Projectile.knockBack, Projectile.owner, 0);
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.width * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var player = Main.player[Projectile.owner];
            var cplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
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

    public class DevilsDevastationBeam : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.timeLeft = 240;
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 8 - 1;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
        }

        public override string Texture => ModContent.GetModProjectile(ModContent.ProjectileType<DemonBlast>()).Texture;

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45);
        }
        public override void AI()
        {
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
            }
            if (!Main.gamePaused && Projectile.timeLeft % 2 == 1)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
            }
        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        private int max = 5;
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
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
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 120);

        }
    }
}