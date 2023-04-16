using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Items.SummonItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.StormWeaver;
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

namespace carnageRework.Items
{
    public class ColdheartIcicle : ModItem
	{
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coldheart Icicle");
            SacrificeTotal = 1;
        }
        public override void SetDefaults()
		{
			    Item.damage = 1;
            Item.DamageType = ModContent.GetModItem(ModContent.ItemType<Carnage>()).Item.DamageType;
			    Item.useTime = 27;
			    Item.useAnimation = Item.useTime;
			    Item.useStyle = ItemUseStyleID.Thrust;
			    Item.noMelee	 = true;
			    Item.noUseGraphic = true;
			    Item.knockBack = 3;
                Item.shoot = ModContent.ProjectileType<ColdheartIcicleProj>();
                Item.autoReuse = true;
                Item.useTurn = false;
            Item.UseSound= SoundID.Item1;
            }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (TooltipLine line in tooltips)
            {
                if (line.Name == "Damage")
                {
                    line.Text = "2% melee damage";
                }
                if (line.Name == "CritChance")
                {
                    line.Text = "Cannot critically hit.";
                }
                if (line.Name == "Speed")
                {
                    line.Text = "seped";
                }
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
        public override bool CanUseItem(Player player)
        {
                if (player.itemTime > 0)
                {
                    return false;
                }
                for (int i = 0; i < 1000; i++)
                {
                    var proj = Main.projectile[i];
                    if (proj.type == ModContent.ProjectileType<ColdheartIcicleProj>() && proj.owner == player.whoAmI && proj.active)
                    {
                        return false;
                    }

                }
            return base.CanUseItem(player);
        }

	}

	public class ColdheartIcicleProj : ModProjectile
	{
        public override string Texture => ModContent.GetModItem(ModContent.ItemType<ColdheartIcicle>()).Texture;

        public override void SetStaticDefaults()
        {
            CalamityLists.pierceResistExceptionList.Add(Projectile.type);
            CalamityLists.projectileDestroyExceptionList.Add(Projectile.type);
        }

        private Vector2 angle;
        public override void SetDefaults()
        {
			Projectile.timeLeft = 10;
            Projectile.width = 24;
			Projectile.height = 24;
            Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = -1;
            Projectile.DamageType = ModContent.GetModItem(ModContent.ItemType<ColdheartIcicle>()).Item.DamageType;
            //Projectile.DamageType = DamageClass.Melee;
           
        }
        public override void OnSpawn(IEntitySource source)
        {
            var player = Main.player[Projectile.owner];
            angle = player.Center.DirectionTo(Main.MouseWorld);
            Projectile.direction = player.direction;
            Projectile.timeLeft = (int)(20/ player.GetWeaponAttackSpeed(ModContent.GetModItem(ModContent.ItemType<TeardropCleaver>()).Item));
        }

        public override void AI()
        {
            var player = Main.player[Projectile.owner];
            Projectile.Center = player.Center + angle * 25  + angle*-Math.Abs(10-Projectile.timeLeft);
            Projectile.rotation = MathHelper.ToRadians(45)+angle.ToRotation();
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
            hitbox.Width = (int)(Projectile.width * Projectile.scale);
            hitbox.Location = (center-new Vector2(hitbox.Width/2,hitbox.Height/2)).ToPoint();

        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            List<int> blacklist = new List<int> 
            {
                ModContent.NPCType<DevourerofGodsBody>(),
                ModContent.NPCType<DevourerofGodsTail>(),
                ModContent.NPCType<ThanatosBody1>(),
                ModContent.NPCType<ThanatosBody2>(),
                ModContent.NPCType<ThanatosTail>(),
                ModContent.NPCType<StormWeaverBody>(),
                NPCID.TheDestroyerBody,
                NPCID.TheDestroyerTail,
            };
            crit = false;
            damage = (int)((target.lifeMax / 50 + target.defense / 2) / (1 - target.Calamity().DR));
            if (target.type == ModContent.NPCType<Providence>())
            {

                damage /= 4;
            }
            if (blacklist.Contains(target.type))
            {
                damage = 1;
            }

            hitDirection = (target.Center.X > Main.player[Projectile.owner].Center.X ? 1 : -1);
        }
        public override void Kill(int timeLeft) {
        }
    }

}