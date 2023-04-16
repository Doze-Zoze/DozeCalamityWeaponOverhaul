using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
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
using System.Data;
using System.Drawing.Text;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Channels;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks
{

    public class AnarchyBladeRework : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<AnarchyBlade>())
            {
                //item.damage = 90;
                //item.useTime = 23*2;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.noMelee = true;
                item.noUseGraphic = true;
                //item.knockBack = 6;
                item.shoot = ModContent.ProjectileType<AnarchyBladeSwordProj>();
                item.autoReuse = true;
                //item.scale = 2f;
                item.useTurn = false;
                item.channel = true;
            }
        }
        public override void HoldItem(Item item, Player player)
        {
        }
        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<AnarchyBlade>())
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
            if (item.type == ModContent.ItemType<AnarchyBlade>())
            {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<AnarchyBladeSwordProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }

                item.useStyle = ItemUseStyleID.Shoot;
            }
            return base.CanUseItem(item, player);
        }

    }

    public class AnarchyBladeSwordProj : ModProjectile
    {
        Vector2 angle = Vector2.Zero;
        private float timer = 0;
        public bool old = false;
        public List<float> oldAngle = new List<float>();
        public float angleDelta = 0;
        public Vector2 oldMouse = Vector2.Zero;
        public int deltaDir = 0;
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<AnarchyBlade>()).Texture;

        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 3;
            Projectile.width = ModContent.GetModItem(ModContent.ItemType<AnarchyBlade>()).Item.width;
            Projectile.height = ModContent.GetModItem(ModContent.ItemType<AnarchyBlade>()).Item.height;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = ModContent.GetModItem(ModContent.ItemType<AnarchyBlade>()).Item.DamageType;
            Projectile.tileCollide = false;

            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;

        }

        public override void OnSpawn(IEntitySource source)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<CarnagePlayer>();

            Projectile.velocity = Vector2.Zero;
            Projectile.extraUpdates = 0;

            var angle1 = player.Center.DirectionTo(Main.MouseWorld);
            if (angle1.X < 0)
            {
                player.direction = -1;
                Projectile.spriteDirection = -1;
            }
            else
            {
                player.direction = 1;
                Projectile.spriteDirection = 1;
            }
            var cplayer = player.GetModPlayer<CarnagePlayer>();
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);

            angle = armCenter.DirectionTo(Main.MouseWorld);
            oldMouse = armCenter - Main.MouseWorld;
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            var angle1 = player.Center.DirectionTo(Main.MouseWorld);
            if (angle1.X < 0)
            {
                player.direction = -1;
                Projectile.spriteDirection = -1;
            }
            else
            {
                player.direction = 1;
                Projectile.spriteDirection = 1;
            }
            var cplayer = player.GetModPlayer<CarnagePlayer>();
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);
            //angle = (armCenter - Main.MouseWorld).SafeNormalize(Vector2.One);


            // angle = angle.RotatedBy(MathHelper.ToRadians(-45*Projectile.spriteDirection));
            //if (player.channel) { angle = player.Center.DirectionTo(Main.MouseWorld); angleDelta *= 0.1f; }
            if (Main.mouseLeft)
            {
                var rawDelta = MathHelper.WrapAngle(oldMouse.ToRotation() - (armCenter - Main.MouseWorld).ToRotation());
                if (player.channel)
                {
                    angleDelta = -rawDelta;
                }
                else
                {

                    angleDelta -= rawDelta > 0 ? Math.Min(rawDelta, MathHelper.ToRadians(3)) : Math.Max(rawDelta, MathHelper.ToRadians(-3));
                }

            }
            else
            {
                //angle = (angle.ToRotation() + (angle.ToRotation() - oldAngle[8])*0.75f).ToRotationVector2();
            }
            angleDelta *= 0.98f;
            oldMouse = armCenter - Main.MouseWorld;
            var maxDelta = 43;
            angle = angle.RotatedBy(angleDelta > 0 ? Math.Min(angleDelta, MathHelper.ToRadians(maxDelta)) : Math.Max(angleDelta, MathHelper.ToRadians(-maxDelta)));
            Projectile.Center = armCenter + angle * 80 * Projectile.scale * new Vector2(1, 1);
            Projectile.rotation = MathHelper.ToRadians(45 * Projectile.spriteDirection) + (armCenter.DirectionTo(Projectile.Center) * Projectile.spriteDirection).ToRotation();


            /*if ((MathHelper.ToDegrees(angleDelta) > 20) || (MathHelper.ToDegrees(angleDelta) < -20))
            {

            }
            if ((MathHelper.ToDegrees(angleDelta) > 10) || (MathHelper.ToDegrees(angleDelta) < -10))
            {
                if (timer % 20 == 0)
                {
                    Projectile.ResetLocalNPCHitImmunity();
                }
            }
            else
            {
                if (timer % 30 == 0)
                {
                    Projectile.ResetLocalNPCHitImmunity();
                }

            }*/
            if (timer % 18 == 0)
            {
                Projectile.ResetLocalNPCHitImmunity();
            }
            /*if (angleDelta > 0)
            {
                if (deltaDir < 0) Projectile.ResetLocalNPCHitImmunity();
                deltaDir = 1;
            } else if (angleDelta< 0)
            {
                if (deltaDir > 0) Projectile.ResetLocalNPCHitImmunity();
                deltaDir = -1;
            }*/
            Projectile.scale = MathHelper.SmoothStep(1, 1.5f, Math.Abs(angleDelta) / MathHelper.ToRadians(80));



            if (!Main.mouseLeft && angleDelta < MathHelper.ToRadians(0.5f) && angleDelta > MathHelper.ToRadians(-0.5f))
            {
                Projectile.Kill();
                player.itemTime = 10;
                player.itemAnimation = player.itemTime;
            }
            else
            {
                Projectile.timeLeft = 60;
                player.itemTime = 10;
                player.itemAnimation = player.itemTime;
            }
            if (Main.mouseRight && timer > 10)
            {
                Projectile.Kill();
                player.itemTime = 10;
                player.itemAnimation = player.itemTime;
            }

            timer++;
            old = true;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));


        }

        public override bool PreDraw(ref Color lightColor)
        {

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
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<BrimstoneBoom>(), damage, knockback, Main.myPlayer);
            if (player.statLife <= player.statLifeMax2 * 0.5f && Main.rand.NextBool(1) && !target.boss && CalamityGlobalNPC.ShouldAffectNPC(target))
            {
                target.life = 0;
                target.HitEffect();
                target.active = false;
                target.NPCLoot();
            }
        }

        public override void Kill(int timeLeft)
        {
        }
    }

}