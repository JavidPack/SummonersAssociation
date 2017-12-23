using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SummonersAssociation.Items;
using ReLogic.Graphics;

namespace SummonersAssociation
{
   public class SummonersAssociation : Mod
   {
      private Dictionary<int, List<int>> m_MinionReferences = new Dictionary<int, List<int>>();

      public SummonersAssociation()
      {
         m_MinionReferences.Add(BuffID.BabySlime, new List<int>() { 266 });
         m_MinionReferences.Add(BuffID.HornetMinion, new List<int>() { 373 });
         m_MinionReferences.Add(BuffID.ImpMinion, new List<int>() { 375 });
         m_MinionReferences.Add(BuffID.SpiderMinion, new List<int>() { 390, 391, 392 });
         m_MinionReferences.Add(BuffID.TwinEyesMinion, new List<int>() { 387, 388 });
         m_MinionReferences.Add(BuffID.PirateMinion, new List<int>() { 393, 394, 395 });
         m_MinionReferences.Add(BuffID.Pygmies, new List<int>() { 191, 192, 193, 194 });
         m_MinionReferences.Add(BuffID.UFOMinion, new List<int>() { 423 });
         m_MinionReferences.Add(BuffID.Ravens, new List<int>() { 317 });
         m_MinionReferences.Add(BuffID.SharknadoMinion, new List<int>() { 407 });
         m_MinionReferences.Add(BuffID.DeadlySphere, new List<int>() { 533 });
         m_MinionReferences.Add(BuffID.StardustDragonMinion, new List<int>() { 626 }); // and 627?
         m_MinionReferences.Add(BuffID.StardustMinion, new List<int>() { 613 });
      }

      public override void AddRecipeGroups()
      {
#pragma warning disable CS0618 // Type or member is obsolete (Lang.misc)
         RecipeGroup group = new RecipeGroup(() => Lang.misc[37].Value + " Minion Staff", new int[]
#pragma warning restore CS0618 // Type or member is obsolete
			{
            ItemID.SlimeStaff,
            ItemID.ImpStaff,
            ItemID.HornetStaff,
            ItemID.SpiderStaff,
            ItemID.OpticStaff,
            ItemID.PirateStaff,
            ItemID.PygmyStaff,
            ItemID.XenoStaff,
            ItemID.RavenStaff,
            ItemID.TempestStaff,
            ItemID.DeadlySphereStaff,
            ItemID.StardustCellStaff,
            ItemID.StardustDragonStaff
         });
         RecipeGroup.RegisterGroup("SummonersAssociation:MinionStaffs", group);

#pragma warning disable CS0618 // Type or member is obsolete (Lang.misc)
         group = new RecipeGroup(() => Lang.misc[37].Value + " Magic Mirror", new int[]
#pragma warning restore CS0618 // Type or member is obsolete
			{
            ItemID.IceMirror,
            ItemID.MagicMirror
         });
         RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
      }

      public override void PostDrawInterface(SpriteBatch spriteBatch)
      {
         if (!Main.LocalPlayer.GetModPlayer<SummonersAssociationCardPlayer>().SummonersAssociationCardInInventory)
         {
            return;
         }

         if (!Main.ingameOptionsWindow && !Main.playerInventory/* && !Main.achievementsWindow*/)
         {
            UpdateBuffText(Main.LocalPlayer);
         }

         if (Main.playerInventory)
         {
            DisplayMaxMinionIcon(Main.LocalPlayer);
         }
      }

      private void UpdateBuffText(Player player)
      {
         float vanillaMinionSlots = 0;
         int xPosition;
         int yPosition;
         Color color;
         int buffsPerLine = 11;
         bool TwoLines = false;
         for (int b = 0; b < 22; ++b)
         {
            if (player.buffType[b] > 0)
            {
               if (b == 11)
               {
                  TwoLines = true;
               }
               int buffID = player.buffType[b];
               xPosition = 32 + b * 38;
               yPosition = 76;
               if (b >= buffsPerLine)
               {
                  xPosition = 32 + (b - buffsPerLine) * 38;
                  yPosition += 50;
               }
               color = new Color(Main.buffAlpha[b], Main.buffAlpha[b], Main.buffAlpha[b], Main.buffAlpha[b]);

               bool isMinionBuff = false;
               int number = 0;

               // Check to see if this buff represents a minion or not
               List<int> minionList;
               if (m_MinionReferences.TryGetValue(buffID, out minionList))
               {
                  isMinionBuff = true;
                  foreach (int minion in minionList)
                  {
                     number = player.ownedProjectileCounts[minion];
                  }

                  if (isMinionBuff)
                  {
                     string text2 = number + " / " + player.maxMinions;
                     if (buffID == BuffID.TwinEyesMinion)
                     {
                        text2 = number + " / " + 2 * player.maxMinions;
                        vanillaMinionSlots += number / 2f;
                     }
                     else
                     {
                        vanillaMinionSlots += number;
                     }
                     Main.spriteBatch.DrawString(Main.fontItemStack, text2, new Vector2((float)xPosition, (float)(yPosition + Main.buffTexture[buffID].Height)), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
                  }
               }
            }
         }
         // Count mod minions.
         //color = new Color(Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1]);
         color = new Color(.4f, .4f, .4f, .4f);
         xPosition = 32;
         yPosition = 76 + 20;
         if (TwoLines)
         {
            yPosition += 50;
         }
         float otherMinions = 0;

         for (int j = 0; j < 1000; j++)
         {
            if (Main.projectile[j].active && Main.projectile[j].owner == player.whoAmI && Main.projectile[j].minion)
            {
               otherMinions += Main.projectile[j].minionSlots;
            }
         }
         otherMinions = otherMinions - vanillaMinionSlots;
         if (otherMinions > 0)
         {
            string modMinionText = "Uncountable mod minions: " + otherMinions + " / " + player.maxMinions;
            Main.spriteBatch.DrawString(Main.fontItemStack, modMinionText, new Vector2((float)xPosition, (float)(yPosition + Main.buffTexture[1].Height)), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
         }
      }

      private void DisplayMaxMinionIcon(Player player)
      {
         if (Main.EquipPage == 0)
         {
            int mH = 0;
            if (Main.mapEnabled)
            {
               if (!Main.mapFullscreen && Main.mapStyle == 1)
               {
                  mH = 256;
               }
               if (mH + 600 > Main.screenHeight)
               {
                  mH = Main.screenHeight - 600;
               }
            }

            int num8 = 6;
            int slot = num8;
            int num9 = Main.screenWidth - 64 - 28;

            float inventoryScale = 0.85f;

            int num11 = mH + (int)((double)(174 + 0) + (double)(slot * 56) * (double)inventoryScale);
            Vector2 vector2_1 = new Vector2((float)(num9 - 10 - 47 - 47 - 14), (float)num11 + (float)Main.inventoryBackTexture.Height * 0.5f);
            Main.spriteBatch.Draw(Main.buffTexture[150], vector2_1, new Microsoft.Xna.Framework.Rectangle?(), Color.White, 0.0f, Utils.Size(Main.buffTexture[150]) / 2f, inventoryScale, SpriteEffects.None, 0.0f);
            Vector2 vector2_2 = Main.fontMouseText.MeasureString(player.maxMinions.ToString());
            Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, player.maxMinions.ToString(), vector2_1 - vector2_2 * 0.5f * inventoryScale, Color.White, 0.0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
            if (Utils.CenteredRectangle(vector2_1, Utils.Size(Main.buffTexture[150])).Contains(new Point(Main.mouseX, Main.mouseY)))
            {
               player.mouseInterface = true;
               string str = "" + player.maxMinions + " Max Minions";
               if (!string.IsNullOrEmpty(str))
                  Main.hoverItemName = str;
            }
         }
      }

      // TODO: summonersAssociation.Call("Buff->Projectile", BuffType("CoolMinionBuff"), ProjectileType("CoolMinionProjectile")); style call.
      //public override object Call(params object[] args)
      //{
      //	return base.Call(args);
      //}
   }
}
