using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System.Linq;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Graphics;

using SummonersAssociation.Models;
using SummonersAssociation.Items;
using Terraria.Localization;
using Terraria.UI;
using SummonersAssociation.UI;
using System;

namespace SummonersAssociation
{
	public class SummonersAssociation : Mod
	{
		private static List<MinionModel> SupportedMinions = new List<MinionModel>() {
			new MinionModel(ItemID.SlimeStaff, BuffID.BabySlime, new List<int>() { 266 }),
			new MinionModel(ItemID.HornetStaff, BuffID.HornetMinion, new List<int>() { 373 }),
			new MinionModel(ItemID.ImpStaff, BuffID.ImpMinion, new List<int>() { 375 }),
			new MinionModel(ItemID.SpiderStaff, BuffID.SpiderMinion, new List<int>() { 390, 391, 392 }),
			new MinionModel(ItemID.OpticStaff, BuffID.TwinEyesMinion, new List<int>() { 387, 388 }),
			new MinionModel(ItemID.PirateStaff, BuffID.PirateMinion, new List<int>() { 393, 394, 395 }),
			new MinionModel(ItemID.PygmyStaff, BuffID.Pygmies, new List<int>() { 191, 192, 193, 194 }),
			new MinionModel(ItemID.XenoStaff, BuffID.UFOMinion, new List<int>() { 423 }),
			new MinionModel(ItemID.RavenStaff, BuffID.Ravens, new List<int>() { 317 }),
			new MinionModel(ItemID.TempestStaff, BuffID.SharknadoMinion, new List<int>() { 407 }),
			new MinionModel(ItemID.DeadlySphereStaff, BuffID.DeadlySphere, new List<int>() { 533 }),
			new MinionModel(ItemID.StardustDragonStaff, BuffID.StardustDragonMinion, new List<int>() { 626 }), // and 627?
            new MinionModel(ItemID.StardustCellStaff, BuffID.StardustMinion, new List<int>() { 613 })
		};

		internal static UserInterface HistoryBookUIInterface;
		internal static HistoryBookUI HistoryBookUI;

		public static SummonersAssociation Instance;

		/// <summary>
		/// Accurate in-UI Mouse position used to spawn UI outside UpdateUI()
		/// </summary>
		public static Vector2 MousePositionUI;

		/// <summary>
		/// Array of the different minion book types. Simple is 0, Normal is 1, Auto is 2
		/// </summary>
		public static int[] BookTypes;

		public SummonersAssociation()
		{ }

		public override void Load() {
			Instance = this;

			if (!Main.dedServ && Main.netMode != 2) {
				HistoryBookUI = new HistoryBookUI();
				HistoryBookUI.Activate();
				HistoryBookUIInterface = new UserInterface();
				HistoryBookUIInterface.SetState(HistoryBookUI);
				HistoryBookUI.redCrossTexture = GetTexture("UI/UIRedCross");
			}
		}

		public override void PostSetupContent()
			//don't change order here (simple is first, normal is second, automatic is third)
			=> BookTypes = new int[] {
				ItemType<MinionHistoryBookSimple>(),
				ItemType<MinionHistoryBook>(),
				ItemType<MinionHistoryBookAuto>()
			};

		public override void Unload() {
			HistoryBookUIInterface = null;
			HistoryBookUI = null;
			HistoryBookUI.redCrossTexture = null;

			BookTypes = null;

			Instance = null;
		}

		public override void AddRecipeGroups()
		{
			List<int> itemList = new List<int>();
			foreach (MinionModel minion in SummonersAssociation.SupportedMinions)
			{
				itemList.Add(minion.ItemID);
			}

			RecipeGroup group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Minion Staff", itemList.ToArray());
			RecipeGroup.RegisterGroup("SummonersAssociation:MinionStaffs", group);

			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Magic Mirror", new int[]
			{
				ItemID.IceMirror,
				ItemID.MagicMirror
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
		}

		public override void UpdateUI(GameTime gameTime) => UpdateHistoryBookUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
			if (inventoryIndex != -1) {
				if (HistoryBookUI.visible) {
					//Remove the item icon when using the item while held outside the inventory (selectedItem == 58)
					int mouseItemIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Item / NPC Head"));
					if (mouseItemIndex != -1) layers.RemoveAt(mouseItemIndex);
					layers.Insert(++inventoryIndex, new LegacyGameInterfaceLayer
						(
						"Summoners Association: History",
						delegate {
							HistoryBookUIInterface.Draw(Main.spriteBatch, new GameTime());
							return true;
						},
						InterfaceScaleType.UI)
					);
				}
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			// Give this to everyone because why not
			if (Main.playerInventory)
			{
				DisplayMaxMinionIcon(Main.LocalPlayer);
			}

			if (!Main.LocalPlayer.GetModPlayer<SummonersAssociationPlayer>().SummonersAssociationCardInInventory)
			{
				return;
			}

			// But only give them the buff info if they carry the card!
			if (!Main.ingameOptionsWindow && !Main.playerInventory/* && !Main.achievementsWindow*/)
			{
				UpdateBuffText(Main.LocalPlayer);
			}
		}

		/// <summary>
		/// Called in UpdateUI
		/// </summary>
		private void UpdateHistoryBookUI(GameTime gameTime) {

			//This is updated to the "in UI" Mouse Position, because the UI itself is spawned in SAPlayer.PreUpdate()
			MousePositionUI = Main.MouseScreen;
			if (HistoryBookUI.visible) HistoryBookUI.Update(gameTime);
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

					int number = 0;

					// Check to see if this buff represents a minion or not
					MinionModel minion = SummonersAssociation.SupportedMinions.SingleOrDefault(minionEntry => minionEntry.BuffID == buffID);
					if (minion != null)
					{
						List<int> projectileList = minion.ProjectileIDs;

						foreach (int projectile in projectileList)
						{
							number += player.ownedProjectileCounts[projectile];
						}

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
