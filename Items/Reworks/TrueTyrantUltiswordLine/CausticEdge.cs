using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Melee;

namespace carnageRework.Items.Reworks.TrueTyrantUltiswordLine
{
    public class CausticAid
    {
        public void CausticExplosion(NPC npc, int dmg, int buffID, Color color, string text = "Neutralized")
        {
            npc.StrikeNPC(dmg, 0, 0);
            npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<CausticCorrosion>()));
            npc.DelBuff(npc.FindBuffIndex(buffID));
        }
        public void CausticExplosion(Player player, int dmg, int buffID, Color color, string text = "Neutralized")
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was careless with chemicals"), dmg, 0);
            player.DelBuff(player.FindBuffIndex(buffID));
        }
    }
    public class CausticCorrosion : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Caustic Corrosion");
            Description.SetDefault("Basically, you're melting");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            if (npc.HasBuff(BuffID.Poisoned))
            {
                npc.life -= 75;
                npc.checkDead();
                npc.DelBuff(buffIndex);
                npc.DelBuff(npc.FindBuffIndex(BuffID.Poisoned));
                CombatText.NewText(npc.Hitbox, Color.Lime, 75);
                for (var i = 0; i < 100; i++)
                {
                    Dust.NewDust(npc.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                }
            }
            if (npc.HasBuff(BuffID.Venom))
            {

                npc.life -= 200;
                npc.checkDead();
                npc.DelBuff(buffIndex);
                npc.DelBuff(npc.FindBuffIndex(BuffID.Venom));
                CombatText.NewText(npc.Hitbox, Color.Violet, 200, dramatic: false, dot: false);
                for (var i = 0; i < 100; i++)
                {
                    Dust.NewDust(npc.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                }
            }
        }
    }

    public class CausticNPC : GlobalNPC
    {

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (npc.HasBuff<CausticCorrosion>())
            {
                drawColor = Color.LightSeaGreen;
            }
        }
    }
    public class CausticEdgeRework : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<CausticEdge>())
            {
                item.damage = 20;
                item.useTime = 20;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.knockBack = 6;
                item.autoReuse = true;
                item.useTurn = true;
            }
        }
        public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (item.type == ModContent.ItemType<CausticEdge>() || item.type == ModContent.ItemType<TrueCausticEdge>())
            {
                if (player.HasBuff(BuffID.WeaponImbuePoison))
                {

                    player.ClearBuff(BuffID.WeaponImbuePoison);
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was careless with chemicals"), 100, 0);
                    for (var i = 0; i < 100; i++)
                    {
                        Dust.NewDust(player.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                    }

                }
                if (player.HasBuff(BuffID.WeaponImbueVenom))
                {
                    player.ClearBuff(BuffID.WeaponImbueVenom);
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was careless with chemicals"), 500, 0);
                    for (var i = 0; i < 100; i++)
                    {
                        Dust.NewDust(player.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
                    }
                }
                target.AddBuff(ModContent.BuffType<CausticCorrosion>(), 60);
            }
        }
    }

}