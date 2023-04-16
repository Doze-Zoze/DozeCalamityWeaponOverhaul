using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks
{
    public class CarnageRework : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<Carnage>())
            {

                item.useTime = 20;
                item.useAnimation = item.useTime;
                item.noMelee = true;
                item.noUseGraphic = true;
                item.shoot = ModContent.ProjectileType<CarnageProj>();
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<Carnage>())
            {
                return true;
            }
            return base.AltFunctionUse(item, player);
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<Carnage>())
            {
                var modplayer = player.GetModPlayer<CarnagePlayer>();
                if (player.altFunctionUse == 2)
                {

                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 3);
                    return false;
                }
                if (modplayer.swingNum == 0)
                {
                    modplayer.swingNum++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0);
                }
                else if (modplayer.swingNum == 1)
                {
                    modplayer.swingNum++;
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1);
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 2);
                    modplayer.swingNum = 0;
                }
                return false;
            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<Carnage>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<CarnageProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }
                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class CarnageProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        private int hitstop = 0;
        private int swingWidth = 180;
        private int swingTime = ModContent.GetModItem(ModContent.ItemType<Carnage>()).Item.useTime;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<Carnage>()).Texture;

        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 240;
            Projectile.width = 50;
            Projectile.height = 54;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = swingTime + 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }
        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Zero;
            Projectile.damage = (int)(Projectile.damage * (1 + (float)modplayer.bloodCount / modplayer.bloodCountMax * 0.5f));
            Projectile.scale = 1.5f + modplayer.bloodCount / (float)modplayer.bloodCountMax * 1.5f;
            if (Projectile.ai[0] == 3)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath10);
                Projectile.damage = Projectile.damage * 2;
                player.Heal(2 * modplayer.bloodCount);

                modplayer.bloodCount = 0;
            }
            swingTime = (int)(swingTime / player.GetWeaponAttackSpeed(ModContent.GetModItem(ModContent.ItemType<Carnage>()).Item));
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            float adust = MathHelper.ToRadians(45 + 180);
            var modplayer = player.GetModPlayer<CarnagePlayer>();

            if (Projectile.ai[0] == 0)
            {

                Projectile.Center = player.Center - (angle * 45 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2)).ToRotation() + adust;

                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 5;
                }
            }


            if (Projectile.ai[0] == 1)
            {
                Projectile.spriteDirection = -1;
                Projectile.Center = player.Center - (angle * 45 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(-MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(-MathHelper.ToRadians(timer * (swingWidth / swingTime) - swingWidth / 2)).ToRotation() + MathHelper.ToRadians(45 + -90);

                if (timer > swingTime)
                {
                    Projectile.Kill();
                    player.itemTime = 5;
                }

            }

            if (Projectile.ai[0] == 2)
            {

                Projectile.Center = player.Center - (angle * 45 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (360 / swingTime / (6 / 4f)) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (360 / swingTime / (6 / 4f)) - swingWidth / 2)).ToRotation() + adust;

                if (timer > swingTime * 2)
                {
                    Projectile.Kill();
                    player.itemTime = 10;
                }

                if ((player.Center - Projectile.Center).X >= 0)
                {
                    player.direction = -1;
                }
                else
                {
                    player.direction = 1;
                }
            }

            if (Projectile.ai[0] == 3)
            {
                Projectile.Center = player.Center - (angle * 45 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(MathHelper.ToRadians(timer * (360 / swingTime) - swingWidth / 2));
                Projectile.rotation = angle.RotatedBy(MathHelper.ToRadians(timer * (360 / swingTime) - swingWidth / 2)).ToRotation() + adust;

                for (var i = 0; i < 10; i++) Dust.NewDust(player.position, player.width, player.height, DustID.Blood, SpeedX: Main.rand.Next(0, 16) * Projectile.rotation.ToRotationVector2().X, SpeedY: Main.rand.Next(0, 16) * Projectile.rotation.ToRotationVector2().Y, Scale: Main.rand.Next(100, 151) / 100f);
                if (timer > swingTime * 4)
                {
                    Projectile.Kill();
                    player.itemTime = 10;
                }

                if (hitstop < 1)
                {
                    timer++;
                }
                else
                {
                    timer += 0.1f;
                    hitstop--;
                }
                if ((player.Center - Projectile.Center).X >= 0)
                {
                    player.direction = -1;
                }
                else
                {
                    player.direction = 1;
                }
            }

            if (hitstop < 1)
            {
                timer++;
            }
            else
            {
                timer += 0.1f;
                //player.velocity = pvel * 0.1f;
                //player.gravity = 0;
                hitstop--;
                /*if (hitstop == 0)
                {
                    player.velocity = pvel;
                }*/
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (player.Center - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }
        private Vector2 pvel = Vector2.Zero;
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(53 * Projectile.scale);
            hitbox.Width = (int)(50 * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();
            if (hitstop == 0)
            {
                pvel = player.velocity;
            }
            hitstop = 10;
            Projectile.timeLeft += 10;
            var nangle = -(player.Center - target.Center).SafeNormalize(Vector2.One);
            for (var i = 0; i < 50; i++)
            {
                Dust.NewDust(target.position, target.width, target.height, DustID.Blood, SpeedX: Main.rand.Next(0, 7) * nangle.X, SpeedY: Main.rand.Next(0, 7) * nangle.Y, Scale: Main.rand.Next(100, 151) / 100f);
            }
            SoundEngine.PlaySound(SoundID.NPCDeath12);
            if (Projectile.ai[0] != 3)
            {
                modplayer.bloodCount++;
                if (modplayer.bloodCount > 10)
                {
                }
                else
                {

                    CombatText.NewText(new Rectangle() { Location = player.position.ToPoint(), Height = player.height, Width = player.width }, Color.PaleVioletRed, modplayer.bloodCount, dot: true, dramatic: modplayer.bloodCount == 10);
                }
            }
        }
    }
}