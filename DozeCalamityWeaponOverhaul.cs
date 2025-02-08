using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Melee;
using DozeCalamityWeaponOverhaul.Reworks.Melee.DefiledGreatswordLine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using ReLogic.Content;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul
{

    public class ToolTipEdits : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            string stack = "ff9999";
            string darkness = "999999";
            string shadow = "ff88ff";
            string daybreak = "f7b359";
            string violence = "f01a41";
            string light = "ffffaa";
            string life = "99ff99";
            string poison = "ccffaa";
            string damage = "ffac9c";
            string movement = "fffadd";
            string defense = "bbbbdd";
            string unique = "ff8888";
            string charge = "b87aff";
            string tc(string hex, string input)
            {
                return $"[c/{hex}:{input}]";
            }
            for (var i = 0; i < tooltips.Count; i++)
            {
                var line = tooltips[i];
                if (line.Name == "Knockback")
                {
                    if (item.type == ModContent.ItemType<TeardropCleaver>()) line.Text = $"{tc(unique, "Unstoppable")} knockback";
                }
                if (line.Name == "Tooltip0")
                {
                    if (item.type == ModContent.ItemType<TaintedBlade>()) line.Text = $"{tc(damage, "Neutralizes")} {tc(poison, "Posion-related debuffs")} on enemies, dealing immense damage\nTrue Melee strikes remove immunities to {tc(poison, "Poison-related debuffs")}\nLeft click to swing the item below this one in your inventory\nRight Click to swing Tainted Blade";
                    if (item.type == ModContent.ItemType<PerfectDark>()) line.Text = $"Encircles you in thick {tc(shadow, "darkness")}\nEnemies take more damage when in the {tc(shadow, "darkness")}\nRight Click to launch a {tc(shadow, "shadow dagger")} that {tc(light, "illuminates")} the {tc(darkness, "darkness")}\n{tc(shadow, "Daggers")} stick to enemies and tiles, and their {tc(light, "light")} decays over time\nThere can only be three {tc(shadow, "daggers")}";
                    if (item.type == ModContent.ItemType<VeinBurster>()) line.Text = $"Fires a toothy mine that will {tc(damage, "explode for immense damage")} after some time\nThe mine sticks to enemies, tiles, and platforms";
                    if (item.type == ModContent.ItemType<CatastropheClaymore>()) line.Text = $"'Fueled by the lost warriors of a crusade.'";
                    if (item.type == ModContent.ItemType<Devastation>()) line.Text = $"Releases immense energy from the souls of those who perished under the Tyrant's command";
                    if (item.type == ModContent.ItemType<ForbiddenOathblade>()) line.Text = $"Fires homing {tc(shadow, "Dusk Scythes")}\nHold Right Click to {tc(charge, "charge up energy")}, releasing it to {tc(movement, "fly towards your cursor")}\nThe dash can {tc(defense, "slam through enemies without taking damage")}";
                    if (item.type == ModContent.ItemType<ExaltedOathblade>()) line.Text = $"{tc(charge, "Charges up")} demonic power over time, which is released when used\nLeft Click shoots a barrage of homing {tc(shadow, "Dusk Scythes")}\nWhen fully charged, releases more powerful {tc(daybreak, "Dawn Scythes")}\nRight Click {tc(movement, "lunges at the cursor,")} {tc(defense, "bouncing off enemies")}\nWhen fully charged, the lunge can {tc(defense, "slam through enemies without taking damage")}";
                    if (item.type == ModContent.ItemType<DevilsDevastation>()) line.Text = $"Combines and empowers the effects of [i:{ModContent.ItemType<ExaltedOathblade>()}] Exalted Oathblade and [i:{ModContent.ItemType<Devastation>()}] Devastation\n'The fury of a damned angel.'";
                    if (item.type == ModContent.ItemType<Carnage>()) line.Text = $"Tears foes asunder with extreme {tc(violence, "violence")}\nStriking will highten your {tc(stack, "bloodlust")}\nEach stack of {tc(stack, "bloodlust")} increases your damage and Carnage's size\nRight Click to release a mighty maelstom, converting all {tc(stack, "bloodlust")} into {tc(life, "healing")}";
                    if (item.type == ModContent.ItemType<Violence>()) line.Text = $"Throws a hellish pitchfork with extreme force\nEnemies are {tc(violence, "violently torn apart")} by the pitchfork\nViolence cannot be thrown again until it finishes it's {tc(violence, "path of bloodshed")}";
                    if (item.type == ModContent.ItemType<Swordsplosion>()) line.Text = $"Fires an army of rainbow swords that chase your cursor";
                    if (item.type == ModContent.ItemType<SeashineSword>()) line.Text = $"Fires a prismatic wave";
                    if (item.type == ModContent.ItemType<CosmicImmaterializer>()) line.Text = $"Summons an exotic dagger to dissassemble your enemies, atom by atom\nMultiple daggers {tc(unique, "syncronize with eachother through their resonance")}";
                }
                if (line.Name == "Tooltip1")
                {
                    if (item.type == ModContent.ItemType<TrueCausticEdge>()) line.Text = $"{tc(damage, "Neutralizes")} {tc(poison, "Posion-related debuffs")} on enemies, dealing immense damage\nTrue Melee strikes remove immunities to {tc(poison, "Poison-related debuffs")}\nLeft click to swing the item below this one in your inventory\nRight Click to swing Caustic Edge";
                    if (item.type == ModContent.ItemType<MajesticGuard>()) line.Text = $"If enemy defense reaches 0, further attacks will {tc(life, "heal you")}\nRight Click to {tc(defense, "parry attacks")}, giving brief invincibility\nAfter a parry, your damage is reduced dramatically but your attack rate is doubled";
                    if (item.type == ModContent.ItemType<DevilsDevastation>() || item.type == ModContent.ItemType<CosmicImmaterializer>())
                    {
                        tooltips.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (item.type == ModContent.ItemType<TitanArm>()) line.Text = $"{tc(charge, "Charge up power")} by dragging the arm across the ground";
                }
                if (line.Name == "Tooltip2")
                {
                    if (item.type == ModContent.ItemType<AnarchyBlade>())
                    {
                        tooltips.Insert(i + 1, new TooltipLine(Mod, "Tooltip4", "Right Click to stop spinning the blade"));
                        i++;
                        continue;
                    }
                    if (item.type == ModContent.ItemType<DevilsDevastation>() || item.type == ModContent.ItemType<CosmicImmaterializer>())
                    {
                        tooltips.RemoveAt(i);
                        i--;
                        continue;
                    }

                }
            }

        }
    }
    public class DozeCalamityWeaponOverhaul : Mod
    {
        private Asset<Texture2D> submarineShockerOriginal = TextureAssets.Item[ModContent.ItemType<SubmarineShocker>()];

        public override void PostSetupContent()
        {
            TextureAssets.Item[ModContent.ItemType<SubmarineShocker>()] = ModContent.Request<Texture2D>("DozeCalamityWeaponOverhaul/Reworks/Melee/SubmarineShocker", (AssetRequestMode)2);
        }
        public override void Load()
        {
            Hooks.Add(new Hook(typeof(TrueCausticEdge).GetMethod("OnHitNPC", BindingFlags.Public | BindingFlags.Instance), (orig_ModItemOnHitNPC orig, ModItem self, Player player, NPC target, NPC.HitInfo hit, int damageDone) =>
            {
                CausticAid.ExplodeCheck(player);
                target.buffImmune[ModContent.BuffType<CausticCorrosion>()] = false;
                target.buffImmune[BuffID.Poisoned] = false;
                target.buffImmune[BuffID.Venom] = false;
                target.buffImmune[ModContent.BuffType<Plague>()] = false;
                target.buffImmune[ModContent.BuffType<SulphuricPoisoning>()] = false;
                target.AddBuff(ModContent.BuffType<CausticCorrosion>(), 240);
            }, true));
            Hooks.Add(new Hook(typeof(TaintedBlade).GetMethod("OnHitNPC", BindingFlags.Public | BindingFlags.Instance), (orig_ModItemOnHitNPC orig, ModItem self, Player player, NPC target, NPC.HitInfo hit, int damageDone) =>
            {
                CausticAid.ExplodeCheck(player);
                target.AddBuff(ModContent.BuffType<CausticCorrosion>(), 120);
            }, true));
            Hooks.Add(new Hook(typeof(CausticEdgeProjectile).GetMethod("OnHitNPC", BindingFlags.Public | BindingFlags.Instance), (orig_ModProjectileOnHitNPC orig, ModProjectile self, NPC target, NPC.HitInfo hit, int damageDone) =>
            {
                self.Projectile.velocity *= 0.5f;

                target.AddBuff(ModContent.BuffType<CausticCorrosion>(), 120);
                CausticAid.ExplodeCheck(Main.player[self.Projectile.owner]);
            }, true));

            //Cosmic Immat on use replacement
            Hooks.Add(new Hook(typeof(CosmicImmaterializer).GetMethod("CanUseItem", BindingFlags.Public | BindingFlags.Instance), (orig_ModItemCanUseItem orig, ModItem self, Player player) =>
            {
                return true;
            }, true));
            Hooks.Add(new Hook(typeof(CosmicImmaterializer).GetMethod("Shoot", BindingFlags.Public | BindingFlags.Instance), (orig_ModItemShoot orig, ModItem self, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) =>
            {
                int p = Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI, 0f, 0f);
                if (Main.projectile.IndexInRange(p))
                    Main.projectile[p].originalDamage = self.Item.damage;
                return false;
            }, true));
        }

        private static List<Hook> Hooks = new List<Hook>();
        private delegate void orig_ModItemOnHitNPC(ModItem self, Player player, NPC npc, NPC.HitInfo hit, int damageDone);
        private delegate void orig_ModProjectileOnHitNPC(ModProjectile self, NPC npc, NPC.HitInfo hit, int damageDone);
        private delegate bool orig_ModItemCanUseItem(ModItem self, Player player);
        private delegate bool orig_ModItemShoot(ModItem self, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback);
    }
}