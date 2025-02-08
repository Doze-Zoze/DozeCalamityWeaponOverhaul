using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee.DefiledGreatswordLine
{
    public class CausticAid
    {
        public static void ExplodeCheck(NPC npc)
        {

            CausticAid.CausticExplosion(npc, 150, Color.LightGreen, BuffID.Poisoned);
            CausticAid.CausticExplosion(npc, 400, Color.Violet, BuffID.Venom);
            if (npc.Calamity().pFlames > 0)
            {
                CausticAid.CausticExplosion(npc, 600, Color.DarkGreen);
                npc.Calamity().pFlames = 0;
            }
            if (npc.Calamity().sulphurPoison > 0)
            {
                CausticAid.CausticExplosion(npc, 10000, Color.YellowGreen);
                npc.Calamity().sulphurPoison = 0;
            }

        }

        public static void ExplodeCheck(Player player)
        {

            CausticAid.CausticExplosion(player, 500, BuffID.WeaponImbueVenom);
            CausticAid.CausticExplosion(player, 100, BuffID.WeaponImbuePoison);
            if (player.Calamity().alchFlask) CausticAid.CausticExplosion(player, 650);
        }
        public static void CausticExplosion(NPC npc, int dmg, Color color, int buffID, string text = "Neutralized")
        {
            if (buffID != -1) if (!npc.HasBuff(buffID)) return;
            npc.life -= dmg;
            npc.checkDead();
            if (npc.HasBuff<CausticCorrosion>()) npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<CausticCorrosion>()));
            npc.DelBuff(npc.FindBuffIndex(buffID));
            CombatText.NewText(npc.Hitbox, color, dmg);
            for (var i = 0; i < 100; i++)
            {
                Dust.NewDust(npc.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
            }
        }
        public static void CausticExplosion(Player player, int dmg, int buffID, string text = "Neutralized")
        {
            if (!player.HasBuff(buffID)) return;
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was careless with chemicals"), dmg, 0);
            player.DelBuff(player.FindBuffIndex(buffID));
            for (var i = 0; i < 100; i++)
            {
                Dust.NewDust(player.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
            }

            ModLoader.TryGetMod("ExampleMod", out Mod exampleMod);
        }

        public static void CausticExplosion(Player player, int dmg, string text = "Neutralized")
        {
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was careless with chemicals"), dmg, 0);
            for (var i = 0; i < 100; i++)
            {
                Dust.NewDust(player.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
            }
        }

        public static void CausticExplosion(NPC npc, int dmg, Color color, string text = "Neutralized")
        {
            npc.life -= dmg;
            npc.checkDead();
            if (npc.HasBuff<CausticCorrosion>()) npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<CausticCorrosion>()));
            CombatText.NewText(npc.Hitbox, color, dmg);
            for (var i = 0; i < 100; i++)
            {
                Dust.NewDust(npc.Center, 0, 0, DustID.Smoke, Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));
            }
        }
    }
    public class CausticCorrosion : ModBuff
    {
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("Caustic Corrosion");
            //Description.SetDefault("Basically, you're melting");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            CausticAid.ExplodeCheck(npc);
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

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[ModContent.ItemType<TaintedBlade>()] = true;

            ItemID.Sets.ItemsThatAllowRepeatedRightClick[ModContent.ItemType<TrueCausticEdge>()] = true;
        }
        public override void SetDefaults(Item item)
        {
            if (item.type == ModContent.ItemType<TaintedBlade>())
            {
                item.damage = 20;
                item.useTime = 15;
                item.scale = 1.5f;
                item.useAnimation = item.useTime;
                item.useStyle = 1;
                item.knockBack = 6;
                item.autoReuse = true;
                item.useTurn = true;
            }
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (item.type == ModContent.ItemType<TaintedBlade>() || item.type == ModContent.ItemType<TrueCausticEdge>())
                return true;
            return base.AltFunctionUse(item, player);
        }

        public override bool CanUseItem(Item item, Player player)
        {

            if (player.altFunctionUse == 0 && player.selectedItem < player.inventory.Length - 10 && (item.type == ModContent.ItemType<TaintedBlade>() || item.type == ModContent.ItemType<TrueCausticEdge>()))
            {
                player.selectedItem += 10;
                player.GetModPlayer<WeaponOverhaulPlayer>().swappedItem = true;
                return false;
            }
            return base.CanUseItem(item, player);
        }


    }

}