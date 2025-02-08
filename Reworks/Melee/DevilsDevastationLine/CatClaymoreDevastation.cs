using CalamityMod.Items.Weapons.Melee;
using DozeCalamityWeaponOverhaul.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DozeCalamityWeaponOverhaul.Reworks.Melee.DevilsDevastationLine
{
    public class CatClay : GlobalItem
    {
        public override void SetDefaults(Item entity)
        {
            if (entity.type == ModContent.ItemType<CatastropheClaymore>())
            {
                entity.damage = 110;
            }

            if (entity.type == ModContent.ItemType<Devastation>())
            {
                entity.damage = 175;
            }
        }

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (item.type == ModContent.ItemType<CatastropheClaymore>())
            {
                var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();

                if (modplayer.swingNum == 0)
                {
                    modplayer.swingNum++;
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SightSoul>(), damage, knockback, player.whoAmI, 0);
                }
                else if (modplayer.swingNum == 1)
                {
                    modplayer.swingNum++;
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MightSoul>(), damage, knockback, player.whoAmI, 1);
                }
                else
                {
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FrightSoul>(), damage, knockback, player.whoAmI, 2);
                    modplayer.swingNum = 0;
                }
                return false;
            }

            if (item.type == ModContent.ItemType<Devastation>())
            {
                var modplayer = player.GetModPlayer<WeaponOverhaulPlayer>();


                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 0);


                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<MightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 1);

                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FrightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 1);

                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 1);

                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 1);

                Projectile.NewProjectile(source, position, velocity * 1.25f, ModContent.ProjectileType<FlightSoul>(), (int)(damage * 0.5f), knockback, player.whoAmI, 2);

                //var starpos = new Vector2(Main.MouseWorld.X + Main.rand.Next(-400, 400 + 1), Main.MouseWorld.Y - Main.screenHeight - 160);
                //Projectile.NewProjectile(source, starpos, starpos.DirectionTo(Main.MouseWorld) * Main.rand.Next(350, 451) / 10 * 1.5f, ModContent.ProjectileType<AstralStar>(), damage, knockback, player.whoAmI, 0);

                return false;

            }
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }

    }
}
