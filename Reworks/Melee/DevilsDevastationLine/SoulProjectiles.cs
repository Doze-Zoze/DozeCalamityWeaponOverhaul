using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee.DevilsDevastationLine
{

    public class SightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void AI()
        {
            Projectile.frame = (int)timer / 4 % 4;
            if (timer > 15 && Projectile.ai[0] != 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(5)), Type, Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(-5)), Type, Projectile.damage, Projectile.knockBack, Projectile.owner, 1);
                Projectile.timeLeft = 0;
            }
            NPC t = Projectile.FindTargetWithinRange(160 * 2.5f);
            if (t != null && Projectile.ai[0] == 1)
            {
                var frag = 64;
                Projectile.velocity = Projectile.velocity.RotatedBy(((Projectile.velocity.SafeNormalize(Vector2.Zero) * (frag - 1) + Projectile.DirectionTo(t.Center)) / frag).ToRotation() - Projectile.velocity.ToRotation());
            }
            timer++;
        }
        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

    public class FrightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void AI()
        {
            Projectile.frame = (int)timer / 4 % 4;
            NPC t = Projectile.FindTargetWithinRange(160 * 2.5f);
            if (t != null)
            {
                var frag = 16;
                Projectile.velocity = (Projectile.velocity * (frag - 1) + Projectile.DirectionTo(t.Center) * 20) / frag;
            }

            timer++;
        }
        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

    public class MightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0.5f;
        }

        public override void AI()
        {
            Projectile.frame = (int)timer / 4 % 4;
            Projectile.velocity *= 1.075f;
            if (Projectile.velocity.Length() > 40)
            {
                Projectile.velocity /= Projectile.velocity.Length() / 10;

            }
            timer++;
        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

    public class FlightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity *= 0.75f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }


        public override void AI()
        {
            Projectile.frame = (int)(timer / 4) % 4;
            Projectile.position += new Vector2(0, (float)Math.Sin((timer + 5) * (Math.PI / 10)) * 30).RotatedBy(Projectile.rotation);
            timer++;


        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

    public class LightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity += new Vector2(0, 15).RotatedBy(Projectile.rotation);
        }

        public override void AI()
        {
            Projectile.frame = (int)timer / 4 % 4;
            Projectile.velocity -= new Vector2(0, 1f).RotatedBy(Projectile.rotation);
            timer++;
        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

    public class NightSoul : ModProjectile
    {
        private float timer = 0;
        public override void SetStaticDefaults()
        {

            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 60 * 5;
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 0;
            Projectile.aiStyle = -2;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity -= new Vector2(0, 15).RotatedBy(Projectile.rotation);
        }

        public override void AI()
        {
            Projectile.frame = (int)timer / 4 % 4;
            Projectile.velocity += new Vector2(0, 1f).RotatedBy(Projectile.rotation);
            timer++;
        }

        List<float> oldProjectileRot = new List<float>();
        List<Vector2> oldProjectilePos = new List<Vector2>();
        List<int> oldProjectileFrame = new List<int>();
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            // Main.EntitySpriteDraw(texture, Projectile.position-Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, 1, SpriteEffects.None, 0);
            int max = 10;
            while (oldProjectileRot.Count < max)
            {
                oldProjectileRot.Add(0);
                oldProjectilePos.Add(Vector2.Zero);
                oldProjectileFrame.Add(0);
            }
            if (!Main.gamePaused)
            {
                oldProjectileRot.Add(Projectile.rotation);
                oldProjectilePos.Add(Projectile.Center);
                oldProjectileFrame.Add(Projectile.frame);

            }
            if (oldProjectileRot.Count > max)
            {
                oldProjectileRot.RemoveAt(0);
                oldProjectilePos.RemoveAt(0);
                oldProjectileFrame.RemoveAt(0);
            }
            for (int i = 0; i < oldProjectileRot.Count; i++)
            {

                var col = (float)Math.Pow(i / (float)max, 2) * Projectile.Opacity;
                if (oldProjectilePos[i] != Vector2.Zero)
                {
                    Main.EntitySpriteDraw(texture, oldProjectilePos[i] - Main.screenPosition, new Rectangle(0, texture.Height / 4 * oldProjectileFrame[i], 22, 22), Color.White * col, oldProjectileRot[i], new Vector2(texture.Size().X / 2, 33 / 2), i / (float)max, SpriteEffects.None, 0);
                }
            }
            //Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void Kill(int timeLeft)
        {
        }
    }

}