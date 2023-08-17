using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SummonersAssociation
{
	[Autoload(Side = ModSide.Client)]
	public class UISystem : ModSystem
	{
		internal static UserInterface LoadoutBookUIInterface;
		internal static LoadoutBookUI LoadoutBookUI;

		public static LocalizedText MinionSlotsBuffText { get; private set; }
		public static LocalizedText UncountableMinionsText { get; private set; }

		public static LocalizedText MinionSlotsIconText { get; private set; }

		public static LocalizedText SentrySlotsIconText { get; private set; }
		public static LocalizedText SentrySlotsIconCountedText { get; private set; }
		public static LocalizedText SentrySlotsIconUncountableText { get; private set; }

		public static LocalizedText LoadoutBookSlotsRequired { get; private set; }
		public static LocalizedText LoadoutBookSelected { get; private set; }
		public static LocalizedText LoadoutBookNotFoundInInventory { get; private set; }
		public static LocalizedText LoadoutBookSummonCountTotal { get; private set; }
		public static LocalizedText LoadoutBookLeftClickClear { get; private set; }
		public static LocalizedText LoadoutBookRightClickCancel { get; private set; }
		public static LocalizedText LoadoutBookLeftClickTwiceClear { get; private set; }
		public static LocalizedText LoadoutBookRightClickSave { get; private set; }

		public static LocalizedText LoadoutBookOnUseNoWeapons { get; private set; }
		public static LocalizedText LoadoutBookOnUseReset { get; private set; }
		public static LocalizedText LoadoutBookOnUseSaved { get; private set; }
		public static LocalizedText LoadoutBookOnUseSelected { get; private set; }

		/// <summary>
		/// Accurate in-UI Mouse position used to spawn UI outside UpdateUI()
		/// </summary>
		public static Vector2 MousePositionUI;

		public override void OnModLoad() {
			LoadoutBookUI = new LoadoutBookUI();
			LoadoutBookUI.Activate();
			LoadoutBookUIInterface = new UserInterface();
			LoadoutBookUIInterface.SetState(LoadoutBookUI);
			LoadoutBookUI.redCrossTexture = SummonersAssociation.Instance.Assets.Request<Texture2D>("UI/UIRedCross", AssetRequestMode.ImmediateLoad);

			string category = $"UI.Buffs.";
			MinionSlotsBuffText ??= GetText(category, "MinionSlots");
			UncountableMinionsText ??= GetText(category, "UncountableMinions");

			category = $"UI.MinionSlotsIcon.";
			MinionSlotsIconText ??= GetText(category, "MinionSlots");

			category = $"UI.SentrySlotsIcon.";
			SentrySlotsIconText ??= GetText(category, "SentrySlots");
			SentrySlotsIconCountedText ??= GetText(category, "Counted");
			SentrySlotsIconUncountableText ??= GetText(category, "Uncountable");

			category = $"UI.LoadoutBook.";
			LoadoutBookSlotsRequired ??= GetText(category, "SlotsRequired");
			LoadoutBookSelected ??= GetText(category, "Selected");
			LoadoutBookNotFoundInInventory ??= GetText(category, "NotFoundInInventory");
			LoadoutBookSummonCountTotal ??= GetText(category, "SummonCountTotal");
			LoadoutBookLeftClickClear ??= GetText(category, "LeftClickClear");
			LoadoutBookRightClickCancel ??= GetText(category, "RightClickCancel");
			LoadoutBookLeftClickTwiceClear ??= GetText(category, "LeftClickTwiceClear");
			LoadoutBookRightClickSave ??= GetText(category, "RightClickSave");

			LoadoutBookOnUseNoWeapons ??= GetText(category, "OnUseNoWeapons");
			LoadoutBookOnUseReset ??= GetText(category, "OnUseReset");
			LoadoutBookOnUseSaved ??= GetText(category, "OnUseSaved");
			LoadoutBookOnUseSelected ??= GetText(category, "OnUseSelected");
		}

		private LocalizedText GetText(string category, string name)
			=> Language.GetOrRegister(Mod.GetLocalizationKey($"{category}{name}"));

		public override void Unload() {
			LoadoutBookUIInterface = null;
			LoadoutBookUI = null;
			LoadoutBookUI.redCrossTexture = null;
			LoadoutBookUI.uiModels?.Clear();
			LoadoutBookUI.itemModels?.Clear();
		}

		public override void UpdateUI(GameTime gameTime) => UpdateLoadoutBookUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
			if (inventoryIndex != -1 && LoadoutBookUI.active) {
				//Remove the item icon when using the item while held outside the inventory (selectedItem == 58)
				int mouseItemIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Item / NPC Head"));
				if (mouseItemIndex != -1) layers.RemoveAt(mouseItemIndex);
				layers.Insert(++inventoryIndex, new LegacyGameInterfaceLayer
					(
					"Summoners' Association: Loadout",
					delegate {
						LoadoutBookUIInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			// Give this to everyone because why not
			if (Main.playerInventory && Config.Instance.InventoryIcon) {
				DisplayMaxMinionIcon(spriteBatch, Main.LocalPlayer);
			}

			if (!Main.LocalPlayer.GetModPlayer<SummonersAssociationPlayer>().SummonersAssociationCardInInventory) {
				return;
			}

			// But only give them the buff info if they carry the card!
			if (!Main.ingameOptionsWindow && !Main.playerInventory/* && !Main.achievementsWindow*/) {
				UpdateBuffText(spriteBatch, Main.LocalPlayer);
			}
		}

		/// <summary>
		/// Called in UpdateUI
		/// </summary>
		private void UpdateLoadoutBookUI(GameTime gameTime) {
			//This is updated to the "in UI" Mouse Position, because the UI itself is spawned in SummonersAssociationPlayer.PreUpdate()
			MousePositionUI = Main.MouseScreen;
			if (LoadoutBookUI.active) LoadoutBookUI.Update(gameTime);
		}

		private void UpdateBuffText(SpriteBatch spriteBatch, Player player) {
			double workingMinions = 0;
			int xPosition;
			int yPosition;
			Color color;
			int buffsPerLine = 11;
			int lineOffset = 0;
			for (int b = 0; b < player.buffType.Length; ++b) {
				int buffID = player.buffType[b];
				if (buffID <= 0) {
					continue;
				}

				// Used outside of minion buff context below
				lineOffset = b / buffsPerLine;

				// Check to see if this buff represents a minion or not
				MinionModel minion = SummonersAssociation.SupportedMinions.SingleOrDefault(minionEntry => minionEntry.BuffID == buffID);
				if (minion == null) {
					continue;
				}

				xPosition = 32 + (b - lineOffset * buffsPerLine) * 38;
				yPosition = 76 + lineOffset * 50 + TextureAssets.Buff[buffID].Height();
				color = new Color(new Vector4(Main.buffAlpha[b]));

				int number = 0;
				double slots = 0;

				var projData = minion.ProjData;

				// Use lowest slot of currently summoned minions so the highest possible total minion count is shown for this buff
				float lowestSlots = float.MaxValue;

				foreach (var data in projData) {
					int num = player.ownedProjectileCounts[data.ProjID];
					float slot = data.Slot;
					if (num > 0 && slot > 0) {
						// Checking for slots existing is required as it's a prerequisite for being concidered a minion (no MinionModel otherwise) and contributing to the minion limit
						// Stardust Dragon has a custom model that sets all segments to 0 except one to 1, don't want it to count as 4/1 minions
						number += num;

						if (slot < lowestSlots)
							lowestSlots = slot;

						slots += num * data.Slot;
					}
				}

				// Projectiles spawn one tick after the buff is applied, showing 0 for a single tick if the buff is fresh
				if (number == 0) continue;

				int newMaxMinions = (int)Math.Floor(player.maxMinions / lowestSlots);
				string ratio = MinionSlotsBuffText.Format(number, newMaxMinions);
				workingMinions += slots;
				spriteBatch.DrawString(FontAssets.ItemStack.Value, ratio, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
			}
			//Count non-registered mod minions
			color = new Color(new Vector4(0.4f));
			xPosition = 32;
			yPosition = 76 + 20 + lineOffset * 50 + TextureAssets.Buff[BuffID.ObsidianSkin].Height();
			double otherMinions = 0;

			for (int j = 0; j < Main.maxProjectiles; j++) {
				Projectile p = Main.projectile[j];
				if (p.active && p.owner == player.whoAmI && p.minion) {
					otherMinions += p.minionSlots;
				}
			}
			otherMinions = Math.Round(otherMinions, 5); //Account for floating point inaccuracy
			otherMinions -= workingMinions;
			//Projectiles spawn one tick after the buff is applied, causing "one tick delay" for otherMinion ?? Fix through ModPlayer
			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
			if (otherMinions > 0 && mPlayer.lastOtherMinions > 0) {
				string modMinionText = UncountableMinionsText.Format(otherMinions, player.maxMinions);
				spriteBatch.DrawString(FontAssets.ItemStack.Value, modMinionText, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
			}
			mPlayer.lastOtherMinions = otherMinions;
		}

		private void DisplayMaxMinionIcon(SpriteBatch spriteBatch, Player player) {
			if (Main.EquipPage != 0) {
				return;
			}
			Texture2D tex = TextureAssets.Buff[BuffID.Bewitched].Value;
			Vector2 size = Utils.Size(tex);

			var defensePos = AccessorySlotLoader.DefenseIconPosition;
			Vector2 drawPos = new Vector2(defensePos.X - 10 - 47 - 47 - 14, defensePos.Y + TextureAssets.InventoryBack.Height() * 0.5f);
			float inventoryScale = 0.85f;
			Vector2 slotOffset = new Vector2(0, -1 * 56 * inventoryScale); //One slot higher
			drawPos += slotOffset;

			Vector2 offset = Config.Instance.Offset;
			offset = new Vector2((offset.X - Config.DefaultX) * Main.screenWidth, (offset.Y - Config.DefaultY) * Main.screenHeight);

			drawPos += offset;
			drawPos.X = Utils.Clamp(drawPos.X, size.X / 2, Main.screenWidth - size.X / 2);
			drawPos.Y = Utils.Clamp(drawPos.Y, size.Y / 2, Main.screenHeight - size.Y / 2);

			spriteBatch.Draw(tex, drawPos, null, Color.White, 0.0f, size / 2f, inventoryScale, SpriteEffects.None, 0f);
			string text = player.maxMinions.ToString();
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 stringLength = font.MeasureString(text);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, drawPos - stringLength * 0.5f * inventoryScale, Color.White, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
			var mouse = new Point(Main.mouseX, Main.mouseY);
			if (Utils.CenteredRectangle(drawPos, size).Contains(mouse)) {
				player.mouseInterface = true;
				string str = MinionSlotsIconText.Format(Math.Round(player.slotsMinions, 2), player.maxMinions);
				if (!string.IsNullOrEmpty(str))
					Main.hoverItemName = str;
			}

			var sentryNameToCount = GetSentryNameToCount(out int sentryCount);

			drawPos.Y -= size.Y * 1.5f;

			tex = TextureAssets.Buff[BuffID.Summoning].Value;

			spriteBatch.Draw(tex, drawPos, null, Color.White, 0.0f, size / 2f, inventoryScale, SpriteEffects.None, 0f);
			text = sentryCount.ToString();
			stringLength = font.MeasureString(text);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, drawPos - stringLength * 0.5f * inventoryScale, Color.White, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
			if (Utils.CenteredRectangle(drawPos, size).Contains(mouse)) {
				player.mouseInterface = true;
				string str = SentrySlotsIconText.Format(sentryCount, player.maxTurrets);
				if (sentryCount > 0) {
					foreach (var item in sentryNameToCount) {
						str += $"\n{SentrySlotsIconCountedText.Format(item.Value, item.Key)}";
					}
				}
				if (!string.IsNullOrEmpty(str))
					Main.hoverItemName = str;
			}
		}

		internal static Dictionary<string, int> GetSentryNameToCount(out int totalCount, bool onlyCount = false) {
			totalCount = 0;

			var sentryNameToCount = new Dictionary<string, int>();
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile p = Main.projectile[i];
				if (p.active && p.sentry && p.owner == Main.myPlayer) {
					totalCount++;
					if (onlyCount) {
						continue;
					}

					string name = Lang.GetProjectileName(p.type).Value;
					if (string.IsNullOrEmpty(name)) {
						name = SentrySlotsIconUncountableText.ToString();
					}

					if (sentryNameToCount.ContainsKey(name)) {
						sentryNameToCount[name]++;
					}
					else {
						sentryNameToCount.Add(name, 1);
					}
				}
			}

			return sentryNameToCount;
		}
	}
}
