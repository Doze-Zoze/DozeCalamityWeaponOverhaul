using carnageRework.Items.Reworks.EarthLine;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace carnageRework.Items
{
    public class CarnagePlayer : ModPlayer
    {
        public int swingNum = 0;
        public int bloodCount = 0;
        private int counter = 0;
        public int bloodCountMax = 10;
        public int DashFrames = 0;
        public int DashType = 0;
        public int ExaltedChargeTime = 0;
        public int ExaltedChargeMax = 75;
        public bool ExaltedFullCharge = true;
        public bool GuardParryUp = false;
        public int ParryTime = 0;
        public int ParryCooldown = 0;
        public int chargeCooldown = 0;
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {

            if (counter < bloodCount)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Blood);
            }
            counter++;
            if (counter > bloodCountMax)
            {
                counter = 0;
            }
            /*if (Main.mouseLeft)
            {
                Texture2D circle = ModContent.Request<Texture2D>("CalVFX/Assets/circle90", (AssetRequestMode)2).Value;
                Main.EntitySpriteDraw(circle, Main.MouseWorld - Main.screenPosition, null, Color.Yellow * 0.33f, 0, circle.Size() / 2, 0.02f, SpriteEffects.None, 0);
            }
            if (Main.mouseRight)
            {
                Texture2D circle = ModContent.Request<Texture2D>("CalVFX/Assets/circle90", (AssetRequestMode)2).Value;
                Main.EntitySpriteDraw(circle, Main.MouseWorld - Main.screenPosition, null, Color.SkyBlue * 0.33f, 0, circle.Size() / 2, 0.02f, SpriteEffects.None, 0);
            }*/

            //if (Player.immune || (Player.immuneTime > 0)) { a = 0; }


        }
        public List<bool> immuneBuffer = new List<bool>() { false };
        public override void ResetEffects()
        {
            
            if (bloodCount > bloodCountMax)
            {
                bloodCount = bloodCountMax;
            }
            if (ExaltedChargeTime > ExaltedChargeMax)
            {
                ExaltedChargeTime = ExaltedChargeMax;
            }
            DashType = 0;
            if (ParryTime > 0) ParryTime--;
            if (ParryCooldown > 0) {
                ParryCooldown--;
                if (ParryCooldown == 0)
                {
                    CombatText.NewText(Player.Hitbox, Color.SkyBlue, "Parry Charged!");
                    SoundEngine.PlaySound(SoundID.Item4);
                }
                    }
            if (chargeCooldown > 0) chargeCooldown--;
            /*var ibufferlen = 60;
            if (Player.immune || (Player.immuneTime > 0)) { immuneBuffer.Add(true); } else { immuneBuffer.Add(false); }
            if (immuneBuffer.Count > ibufferlen) immuneBuffer.RemoveAt(0);
            if (immuneBuffer.Count > ibufferlen) immuneBuffer.RemoveAt(0);
            var iframes = 0;
            for (var i = 0; i < immuneBuffer.Count; i++)
            {
                if (immuneBuffer[i]) iframes++;
            }*/
            /*if (Player.itemAnimation > 1)
            {
                Player.itemAnimation = 1;
                Player.itemTime = 1;
            }*/
            /*if (Player.itemAnimation==1)
            {
                Player.position.X += 16 * 6;
                if(Player.position.X > 19300)
                {
                    Player.position.X -= (16 * 6 * 26.5f);
                    Player.position.Y -= 16 * 5;
                }
            }*/
            //Main.NewText((iframes / (float)ibufferlen) *100);
            //if (Player.wingTime < 1) Player.wingTime = 1;
            //Main.NewText(Player.wingTime);

        }
        public override void PreUpdate()
        {
            if (!ExaltedFullCharge && ExaltedChargeTime >= ExaltedChargeMax)
            {
                ExaltedFullCharge = true;
                SoundEngine.PlaySound(SoundID.Item103);
                for (var i = 0; i < 20;i++) { 
                    Dust.NewDust(Player.position,Player.width,Player.height, DustID.Shadowflame, Scale: 1.5f);
                }
            }
            if (ExaltedChargeTime <= 5)
            {
                ExaltedFullCharge = false;
            }
            if (DashFrames > 0)
            {
                Player.velocity = (Player.velocity + Player.DirectionTo(Main.MouseWorld) * Player.Center.Distance(Main.MouseWorld) / 4) / 2;
                if (Player.velocity.Length() > 20)
                {
                    Player.velocity /= Player.velocity.Length() / 20;
                }
                Player.gravity = 0;
                if (DashType == 0) { 
                    Player.SetImmuneTimeForAllTypes(3);
                }
                Player.immuneNoBlink = true;
                DashFrames--;
            }
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
            if (ParryTime > 0)
            {
                Player.AddBuff(ModContent.BuffType<GuardParry>(), 60*5);
                ParryCooldown = 60 * 10;
                CombatText.NewText(Player.Hitbox, Color.SkyBlue, "Parried!", true);
                Player.SetImmuneTimeForAllTypes((Player.longInvince ? 30 : 15));

                return false;
            }
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }
    }
}