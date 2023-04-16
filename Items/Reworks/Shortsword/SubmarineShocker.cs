
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Melee.Shortswords;
using carnageRework.Common;
using Microsoft.Xna.Framework;
using System.Data;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items.Reworks.Shortsword
{
    public class SubmarineShockerRework : GlobalItem
    {
        public override void SetDefaults(Item item) 
        {
            if (item.type == ModContent.ItemType<SubmarineShocker>())
            {
                item.useTime = 15;
                item.useAnimation = item.useTime;
                item.width = 54;
                item.height = 54;
                item.shoot = ModContent.ProjectileType<SubmarineShockerSwordProj>();
            }
        }
    }
    public class SubmarineShockerSwordProj : ModProjectile
    {
        public Vector2 angle = Vector2.Zero;
        public float swingTime = 15;
        public int timer = 0;
        public float swingRadius = 135;

        public override string Texture => "carnageRework/Items/Reworks/Shortsword/SubmarineShocker";
        public override void SetDefaults()
        {
            Projectile.timeLeft = 2400;
            Projectile.width = 54;
            Projectile.height = 54;
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
            angle = -Main.player[Projectile.owner].Center.DirectionTo(Main.MouseWorld);
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


            var angle2 = MathHelper.ToRadians(MathHelper.SmoothStep(-swingRadius / 2, swingRadius / 2, timer / swingTime));
            Projectile.Center = armCenter - (angle * 45 * (1 + (Projectile.scale - 1) * 0.75f)).RotatedBy(Projectile.spriteDirection * angle2);
            Projectile.rotation = angle.RotatedBy(Projectile.spriteDirection * angle2).ToRotation() + adust;


            if (timer > swingTime)
            {
                Projectile.Kill();
                player.itemTime = 0;
                player.itemAnimation = 0;
            } else
            {
                player.itemTime = 10;
                player.itemAnimation = 10;
            }
            timer++;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (armCenter - Projectile.Center).ToRotation() + MathHelper.ToRadians(90));
            if (chargeCooldown> 0) { chargeCooldown--; }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
        public int chargeCooldown = 0;
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var chargeCooldown = Main.player[Projectile.owner].GetModPlayer<CarnagePlayer>().chargeCooldown;
            target.AddBuff(BuffID.Electrified, 60 * 5);
            
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, -Vector2.UnitY * 2, ModContent.ProjectileType<Spark>(), (int)((float)damage * 0.7f * (crit? 0.5f : 1)), knockback, Main.myPlayer);
            if (chargeCooldown == 0)
            {
                TryToSuperchargeNPC(target);
                Main.player[Projectile.owner].GetModPlayer<CarnagePlayer>().chargeCooldown = 60;
                for (int i = 0; i < Main.npc.Length; i++)
                {
                    if (i != target.whoAmI && Main.npc[i].CanBeChasedBy(base.Projectile) && Main.npc[i].Distance(target.Center) < 240f && TryToSuperchargeNPC(Main.npc[i]))
                    {
                        for (float increment = 0f; increment <= 1f; increment += 0.1f)
                        {
                            Dust dust = Dust.NewDustPerfect(Vector2.Lerp(target.Center, Main.npc[i].Center,increment), DustID.Electric);
                            dust.velocity = -dust.position.DirectionTo(target.Center).RotatedBy(MathHelper.ToRadians(MathHelper.Lerp(-50,50, increment)))*10;
                            dust.scale = 1f;
                            dust.noGravity = true;
                        }
                    }
                }
            }

            
        }

        public bool TryToSuperchargeNPC(NPC npc)
        {
            for (int i = 0; i < Main.projectile.Length; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<VoltageStream>() && Main.projectile[i].ai[1] == (float)i)
                {
                    return false;
                }
            }
            Projectile.NewProjectileDirect(base.Projectile.GetSource_FromThis(), npc.Center, -Vector2.UnitY*2, ModContent.ProjectileType<Spark>(), (int)((double)base.Projectile.damage * 0.8), 0f, base.Projectile.owner, 0f, npc.whoAmI);
            return true;
        }
    }
}