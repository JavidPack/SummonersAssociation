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
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SummonersAssociation
{
	public class UISystem : ModSystem
	{
		internal static UserInterface HistoryBookUIInterface;
		internal static HistoryBookUI HistoryBookUI;

		/// <summary>
		/// Accurate in-UI Mouse position used to spawn UI outside UpdateUI()
		/// </summary>
		public static Vector2 MousePositionUI;

		public override void OnModLoad() {
			if (!Main.dedServ && Main.netMode != NetmodeID.Server) {
				HistoryBookUI = new HistoryBookUI();
				HistoryBookUI.Activate();
				HistoryBookUIInterface = new UserInterface();
				HistoryBookUIInterface.SetState(HistoryBookUI);
				HistoryBookUI.redCrossTexture = SummonersAssociation.Instance.Assets.Request<Texture2D>("UI/UIRedCross", AssetRequestMode.ImmediateLoad);
			}
		}

		public override void Unload() {
			HistoryBookUIInterface = null;
			HistoryBookUI = null;
			HistoryBookUI.redCrossTexture = null;
			HistoryBookUI.uiModels?.Clear();
			HistoryBookUI.itemModels?.Clear();
		}

		public override void UpdateUI(GameTime gameTime) => UpdateHistoryBookUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
			if (inventoryIndex != -1) {
				if (HistoryBookUI.active) {
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

		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			// Give this to everyone because why not
			if (Main.playerInventory) {
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
		private void UpdateHistoryBookUI(GameTime gameTime) {
			//This is updated to the "in UI" Mouse Position, because the UI itself is spawned in SummonersAssociationPlayer.PreUpdate()
			MousePositionUI = Main.MouseScreen;
			if (HistoryBookUI.active) HistoryBookUI.Update(gameTime);
		}

		private void UpdateBuffText(SpriteBatch spriteBatch, Player player) {
			double workingMinions = 0;
			int xPosition;
			int yPosition;
			Color color;
			int buffsPerLine = 11;
			int lineOffset = 0;
			for (int b = 0; b < player.buffType.Length; ++b) {
				if (player.buffType[b] > 0) {
					lineOffset = b / buffsPerLine;
					int buffID = player.buffType[b];
					xPosition = 32 + (b - lineOffset * buffsPerLine) * 38;
					yPosition = 76 + lineOffset * 50 + TextureAssets.Buff[buffID].Height();
					color = new Color(new Vector4(Main.buffAlpha[b]));

					int number = 0;
					double slots = 0;

					// Check to see if this buff represents a minion or not
					MinionModel minion = SummonersAssociation.SupportedMinions.SingleOrDefault(minionEntry => minionEntry.BuffID == buffID);
					if (minion != null) {
						List<int> projectileList = minion.ProjectileIDs;

						for (int i = 0; i < minion.ProjectileIDs.Count; i++) {
							int num = player.ownedProjectileCounts[minion.ProjectileIDs[i]];
							if (num > 0) {
								number += num;
								slots += num * minion.Slots[i];
							}
						}
						//Projectiles spawn one tick after the buff is applied, showing 0 for a single tick if the buff is fresh
						if (number == 0) continue;

						//Use lowestSlots so the highest possible minion count is shown for this buff
						//edge case 0, if for whatever reason a mod manually assigns 0 as the slot, it will turn it to 1
						float lowestSlots = minion.Slots.Min(x => x == 0 ? 1 : x);
						int newMaxMinions = (int)Math.Floor(player.maxMinions / lowestSlots);
						string ratio = number + " / " + newMaxMinions;
						// TODO: 7/8 shown for spider minions with stardust armor. Technically there is .75 slots left, but StaffMinionSlotsRequired defaults to 1 and is an int. Might need to do the math and show 1 less if available minion slots is less than 1.
						workingMinions += slots;
						spriteBatch.DrawString(FontAssets.ItemStack.Value, ratio, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
					}
				}
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
			otherMinions -= workingMinions;
			//Projectiles spawn one tick after the buff is applied, causing "one tick delay" for otherMinion ?? Fix through ModPlayer
			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
			if (otherMinions > 0 && mPlayer.lastOtherMinions > 0) {
				string modMinionText = "Uncountable mod minion slots: " + otherMinions + " / " + player.maxMinions;
				spriteBatch.DrawString(FontAssets.ItemStack.Value, modMinionText, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
			}
			mPlayer.lastOtherMinions = otherMinions;
		}

		private void DisplayMaxMinionIcon(SpriteBatch spriteBatch, Player player) {
			if (Main.EquipPage == 0) {
				int mH = 0;
				if (Main.mapEnabled) {
					if (!Main.mapFullscreen && Main.mapStyle == 1) {
						mH = 256;
					}
					if (mH + 600 > Main.screenHeight) {
						mH = Main.screenHeight - 600;
					}
				}

				int six = 6;
				int slot = six;
				int x = Main.screenWidth - 64 - 28;

				float inventoryScale = 0.85f;

				int y = mH + (int)(174 + 0 + slot * 56 * inventoryScale);

				Texture2D tex = TextureAssets.Buff[BuffID.Bewitched].Value;
				Vector2 size = Utils.Size(tex);

				//Vector2 vector2_1 = new Vector2(num9 - 10 - 47 - 47 - 14, num11 + Main.inventoryBackTexture.Height * 0.5f);
				//- 10 - 47 - 47 - 14: x offset from the right edge of the screen
				//+ Main.inventoryBackTexture.Height * 0.5f: y offset so its aligned with an inv slot
				Vector2 drawPos = new Vector2(x - 10 - 47 - 47 - 14, y + TextureAssets.InventoryBack.Height() * 0.5f);

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
					string str = "" + Math.Round(player.slotsMinions, 2) + " / " + player.maxMinions + " Minion Slots";
					if (!string.IsNullOrEmpty(str))
						Main.hoverItemName = str;
				}

				int sentryCount = 0;
				var sentryNameToCount = new Dictionary<string, int>();
				for (int i = 0; i < Main.maxProjectiles; i++) {
					Projectile p = Main.projectile[i];
					if (p.active && p.sentry && p.owner == Main.myPlayer) {
						sentryCount++;
						string name = Lang.GetProjectileName(p.type).Value;
						if (string.IsNullOrEmpty(name)) {
							name = "Uncountable";
						}
						if (sentryNameToCount.ContainsKey(name)) {
							sentryNameToCount[name]++;
						}
						else {
							sentryNameToCount.Add(name, 1);
						}
					}
				}

				drawPos.Y -= size.Y * 1.5f;

				tex = TextureAssets.Buff[BuffID.Summoning].Value;

				spriteBatch.Draw(tex, drawPos, null, Color.White, 0.0f, size / 2f, inventoryScale, SpriteEffects.None, 0f);
				text = sentryCount.ToString();
				stringLength = font.MeasureString(text);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, drawPos - stringLength * 0.5f * inventoryScale, Color.White, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
				if (Utils.CenteredRectangle(drawPos, size).Contains(mouse)) {
					player.mouseInterface = true;
					string str = "" + sentryCount + " / " + player.maxTurrets + " Sentry Slots";
					if (sentryCount > 0) {
						foreach (var item in sentryNameToCount) {
							str += $"\n{item.Value}: {item.Key}";
						}
					}
					if (!string.IsNullOrEmpty(str))
						Main.hoverItemName = str;
				}
			}
		}
	}
}
