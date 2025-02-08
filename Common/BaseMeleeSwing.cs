using CalamityMod;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Common
{
    public abstract class BaseMeleeItem : GlobalItem
    {
        public virtual int ProjectileType { get; set; }
        public virtual int ItemType { get; set; }
        public virtual bool SizeModifiers { get; set; } = true;

        public virtual bool RClickAutoswing { get; set; } = false;

        public virtual void Defaults(Item item) { }

        public override void SetStaticDefaults()
        {
            if (RClickAutoswing) ItemID.Sets.ItemsThatAllowRepeatedRightClick[ItemType] = true;
        }

        public override bool AllowPrefix(Item item, int pre)
        {
            if (Item.GetVanillaPrefixes(PrefixCategory.Melee).Contains(pre) && SizeModifiers)
            {
                return true;
            }
            return base.AllowPrefix(item, pre);
        }
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemType)
            {
                item.noMelee = true;
                item.noUseGraphic = true;
                item.shoot = ProjectileType;
                item.autoReuse = true;
                item.useTurn = false;
                item.useStyle = ItemUseStyleID.Shoot;
                if (SizeModifiers) PrefixLegacy.ItemSets.SwordsHammersAxesPicks[item.type] = true;
                Defaults(item);
            }
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
            }
            return base.CanUseItem(item, player);
        }
    }
    public abstract class BaseMeleeSwing : ModProjectile
    {
        public virtual Vector2 angle { get; set; } = Vector2.Zero;
        public virtual int timer { get; set; } = 0;
        public virtual int swingWidth { get; set; } = 180;
        public virtual int swingTime { get; set; } = 20;
        public virtual bool AlternateSwings { get; set; } = true;
        public virtual int OffsetDistance { get; set; } = 0;

        public virtual Item BaseItem { get; set; }
        public virtual bool UsesBaseItem { get; set; } = true;

        public virtual int AfterImageLength { get; set; } = 0;

        public virtual bool useMeleeSpeed { get; set; } = true;

        public virtual bool useMeleeSize { get; set; } = true;

        public static Asset<Texture2D> TrailTexture;

        public virtual Color[] trailColors { get; set; } = [Color.White, Color.Black];

        public virtual bool drawSwordTrail { get; set; } = false;

        public float trailOffset = 25;
        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = swingTime * 2;
            if (UsesBaseItem)
            {
                Projectile.width = BaseItem.width;
                Projectile.height = BaseItem.height;
            }
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = ModLoader.GetMod("CalamityMod").Find<DamageClass>("TrueMeleeDamageClass");
            Projectile.tileCollide = false;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 100;
            Defaults();


        }
        List<float> oldProjectileRot = new List<float> { };
        List<Vector2> oldProjectilePos = new List<Vector2> { };
        public override void OnSpawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            angle = (player.Center - Main.MouseWorld).SafeNormalize(Vector2.One);
            Projectile.velocity = Vector2.Zero;
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
            Spawn(source);
            if (useMeleeSpeed)
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
            if (useMeleeSize)
            {
                if (player.meleeScaleGlove) Projectile.scale *= 1.1f;
                Projectile.scale *= player.HeldItem.scale;
            }
        }
        /// <summary>
        /// Happens after movement but before timer increases. Use as to not cancel the default AI behavior.
        /// </summary>
        public virtual void AdditionalAI() { }
        /// <summary>
        /// happens after OnSpawn. Use as not to cancel the default OnSpawn behavior.
        /// </summary>
        /// <param name="source"></param>
        public virtual void Spawn(IEntitySource source) { }
        /// <summary>
        /// happens after SetDefaults. Use as not to cancel default SetDefaults behavior.
        /// </summary>
        public virtual void Defaults() { }
        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            float adust = MathHelper.ToRadians(45 + 180);
            if (Projectile.spriteDirection == -1)
            {
                adust = MathHelper.ToRadians(-45);
            }
            var armCenter = player.Center - new Vector2(5 * player.direction, 2);
            if (AfterImageLength > 0)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                if (oldProjectileRot.Count > AfterImageLength)
                {
                    oldProjectileRot.RemoveAt(0);
                    oldProjectilePos.RemoveAt(0);
                }
            }
            var angle2 = (AlternateSwings && (modplayer.swingNum % 2 == 1 ? false : true) ? SwingFunction() : -SwingFunction());
            Projectile.Center = armCenter - (angle * OffsetDistance * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
            Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;

            AdditionalAI();

            player.itemTime = swingTime + 1 - timer;
            if (timer > swingTime)
            {
                Projectile.Kill();
                player.itemTime = 0;
            }
            timer++;
            var armDir = armCenter - Projectile.Center;
            armDir.Y *= player.gravDir;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armDir.ToRotation() + MathHelper.ToRadians(90));
        }
        /// <summary>
        /// Returns the offset from the centerpoint in radians. Automatically will be inverted if AlternateSwings is enabled.
        /// </summary>
        /// <returns></returns>
        public virtual float SwingFunction()
        {
            return MathHelper.ToRadians(MathHelper.SmoothStep(-swingWidth / 2, swingWidth / 2, timer / (float)swingTime));
        }
        public override bool PreDraw(ref Color lightColor)
        {

            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();
            if (AfterImageLength > 0)
            {
                Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
                for (int i = 0; i < oldProjectileRot.Count; i++)
                {
                    var col = Projectile.Opacity * (i / (float)AfterImageLength) * 0.1f;
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
            if (drawSwordTrail)
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
                GameShaders.Misc["CalamityMod:ExobladeSlash"].UseColor(trailColors[0]);

                GameShaders.Misc["CalamityMod:ExobladeSlash"].UseSecondaryColor(trailColors[1]);

                GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(trailColors[2].ToVector3());

                GameShaders.Misc["CalamityMod:ExobladeSlash"].Shader.Parameters["flipped"].SetValue((AlternateSwings && modplayer.swingNum % 2 == 1) ^ Projectile.spriteDirection == -1 ? false : true);
                GameShaders.Misc["CalamityMod:ExobladeSlash"].Apply();

                var positionsToUse = Projectile.oldPos.Take(25).ToArray();
                for (var i = 0; i < positionsToUse.Length; i++)
                {
                    if (i >= timer) continue;
                    positionsToUse[i] += (Projectile.oldRot[i] - MathHelper.PiOver4 * (Projectile.spriteDirection == -1 ? 3 : 1)).ToRotationVector2() * 20;
                }
                PrimitiveRenderer.RenderTrail(positionsToUse, new(trailWidth, trailColor, (_) => trailOffset, shader: GameShaders.Misc["CalamityMod:ExobladeSlash"]), 25);
                Main.spriteBatch.ExitShaderRegion();

                Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            }
            Main.player[Projectile.owner].heldProj = Projectile.whoAmI;
            return true;
        }

        public virtual float trailWidth(float comp)
        {
            return MathHelper.Lerp(10, 0, comp);
        }

        public virtual Color trailColor(float comp)
        {
            return new Color(100, 100, 100);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            var center = hitbox.Center.ToVector2();
            hitbox.Height = (int)(Projectile.height * Projectile.scale);
            hitbox.Width = (int)(Projectile.width * Projectile.scale);
            hitbox.Location = (center - new Vector2(hitbox.Width / 2, hitbox.Height / 2)).ToPoint();

        }
        /// <summary>
        /// Spawns projectiles from the sword swing, automatically spacing them out on the if an amount is set.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="velocity">Velocity of the projectile. Automatically converted into a Vector2 in the direction of swing.</param>
        /// <param name="damagemod">The modifier from the source projectile's damage for this projectile</param>
        /// <param name="amount">The amount of projectiles for the sword to shoot. If set, it will space out those projectiles evenly. If unset, will force a shot.</param>
        /// <param name="negate"></param>
        /// <returns></returns>
        public void shootCheck(int type = 0, float velocity = 1, float damagemod = 1, int amount = 0, int negate = 0, int ai0 = 0)
        {
            if (negate == 0)
            {
                negate = Projectile.spriteDirection;
            }
            if (amount == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * velocity, type, (int)(Projectile.damage * damagemod), Projectile.knockBack, Projectile.owner, ai0);
                return;
            }
            amount += 1;
            if (timer % (swingTime / amount) == 0 && timer > 0 && timer < swingTime - swingTime / amount / 2)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Projectile.rotation + negate * MathHelper.ToRadians(45 - negate * 90)).ToRotationVector2() * velocity, type, (int)(Projectile.damage * damagemod), Projectile.knockBack, Projectile.owner, ai0);
            }
        }
    }

}