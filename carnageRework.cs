using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace carnageRework
{
	public class carnageRework : Mod
	{
        private Asset<Texture2D> submarineShockerOriginal = TextureAssets.Item[ModContent.ItemType<SubmarineShocker>()];

        public override void PostSetupContent()
        {
            TextureAssets.Item[ModContent.ItemType<SubmarineShocker>()] = ModContent.Request<Texture2D>("carnageRework/Items/Reworks/Shortsword/SubmarineShocker", (AssetRequestMode)2);
        }

        public override void Unload()
        {
            TextureAssets.Item[ModContent.ItemType<SubmarineShocker>()] = submarineShockerOriginal;
        }
    }
}