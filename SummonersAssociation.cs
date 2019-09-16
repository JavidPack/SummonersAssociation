using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using SummonersAssociation.Items;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SummonersAssociation
{
	public class SummonersAssociation : Mod
	{
		private static List<MinionModel> SupportedMinions;

		private static List<int> ModdedSummonerWeaponsWithExistingBuff;

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

		public SummonersAssociation() { }

		public override void Load() {
			Instance = this;

			if (!Main.dedServ && Main.netMode != 2) {
				HistoryBookUI = new HistoryBookUI();
				HistoryBookUI.Activate();
				HistoryBookUIInterface = new UserInterface();
				HistoryBookUIInterface.SetState(HistoryBookUI);
				HistoryBookUI.redCrossTexture = GetTexture("UI/UIRedCross");
			}

			SupportedMinions = new List<MinionModel>() {
				new MinionModel(ItemID.SlimeStaff, BuffID.BabySlime, ProjectileID.BabySlime),
				new MinionModel(ItemID.HornetStaff, BuffID.HornetMinion, ProjectileID.Hornet),
				new MinionModel(ItemID.ImpStaff, BuffID.ImpMinion, ProjectileID.FlyingImp),
				new MinionModel(ItemID.SpiderStaff, BuffID.SpiderMinion, new List<int>() { ProjectileID.VenomSpider, ProjectileID.JumperSpider, ProjectileID.DangerousSpider }),
				new MinionModel(ItemID.OpticStaff, BuffID.TwinEyesMinion, new List<int>() { ProjectileID.Retanimini, ProjectileID.Spazmamini }),
				new MinionModel(ItemID.PirateStaff, BuffID.PirateMinion, new List<int>() { ProjectileID.OneEyedPirate, ProjectileID.SoulscourgePirate, ProjectileID.PirateCaptain }),
				new MinionModel(ItemID.PygmyStaff, BuffID.Pygmies, new List<int>() { ProjectileID.Pygmy, ProjectileID.Pygmy2, ProjectileID.Pygmy3, ProjectileID.Pygmy4 }),
				new MinionModel(ItemID.XenoStaff, BuffID.UFOMinion, new List<int>() { ProjectileID.UFOMinion }),
				new MinionModel(ItemID.RavenStaff, BuffID.Ravens, new List<int>() { ProjectileID.Raven }),
				new MinionModel(ItemID.TempestStaff, BuffID.SharknadoMinion, new List<int>() { ProjectileID.Tempest }),
				new MinionModel(ItemID.DeadlySphereStaff, BuffID.DeadlySphere, new List<int>() { ProjectileID.DeadlySphere }),
				new MinionModel(ItemID.StardustDragonStaff, BuffID.StardustDragonMinion, ProjectileID.StardustDragon2, 1f),
				new MinionModel(ItemID.StardustCellStaff, BuffID.StardustMinion, new List<int>() { ProjectileID.StardustCellMinion })
			};

			ModdedSummonerWeaponsWithExistingBuff = new List<int>();
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

			SupportedMinions = null;
			BookTypes = null;

			Instance = null;
		}

		public override void AddRecipeGroups() {
			var itemList = new List<int>();
			foreach (MinionModel minion in SupportedMinions) {
				itemList.Add(minion.ItemID);
			}

			foreach (int type in ModdedSummonerWeaponsWithExistingBuff) {
				itemList.Add(type);
			}
			ModdedSummonerWeaponsWithExistingBuff = null;

			var group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Minion Staff", itemList.ToArray());
			RecipeGroup.RegisterGroup("SummonersAssociation:MinionStaffs", group);

			group = new RecipeGroup(() => Language.GetTextValue("LegacyMisc.37") + " Magic Mirror", new int[]
			{
				ItemID.MagicMirror,
				ItemID.IceMirror
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
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
					if (b % 11 == 0) {
						lineOffset = b / 11;
					}
					int buffID = player.buffType[b];
					xPosition = 32 + b * 38;
					yPosition = 76;
					if (b >= buffsPerLine) {
						xPosition = 32 + (b - buffsPerLine) * 38;
						yPosition += lineOffset * 50;
					}
					color = new Color(new Vector4(Main.buffAlpha[b]));

					int number = 0;
					double slots = 0;

					// Check to see if this buff represents a minion or not
					MinionModel minion = SupportedMinions.SingleOrDefault(minionEntry => minionEntry.BuffID == buffID);
					if (minion != null) {
						List<int> projectileList = minion.ProjectileIDs;
						List<double> slotList = minion.Slots;

						for (int i = 0; i < minion.ProjectileIDs.Count; i++) {
							int num = player.ownedProjectileCounts[minion.ProjectileIDs[i]];
							if (num > 0) {
								number += num;
								slots += num * minion.Slots[i];
							}
						}

						//Use lowestSlots so the highest possible minion count is shown for this buff
						//edge case 0, if for whatever reason a mod manually assigns 0 as the slot, it will turn it to 1
						double lowestSlots = minion.Slots.Min(x => x == 0? 1 : x);
						int newMaxMinions = (int)Math.Floor(player.maxMinions / lowestSlots);
						string ratio = number + " / " + newMaxMinions;
						workingMinions += slots;
						spriteBatch.DrawString(Main.fontItemStack, ratio, new Vector2(xPosition, yPosition + Main.buffTexture[buffID].Height), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
					}
				}
			}
			// Count mod minions.
			//color = new Color(Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1]);
			color = new Color(new Vector4(0.4f));
			xPosition = 32;
			yPosition = 76 + 20;
			yPosition += lineOffset * 50;
			double otherMinions = 0;

			for (int j = 0; j < 1000; j++) {
				if (Main.projectile[j].active && Main.projectile[j].owner == player.whoAmI && Main.projectile[j].minion) {
					otherMinions += Main.projectile[j].minionSlots;
				}
			}
			otherMinions -= workingMinions;
			//buff gets applied one tick after the projectile is spawned, causing "one tick delay" for otherMinion ?? Hotfix through ModPlayer
			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
			if (otherMinions > 0 && mPlayer.lastOtherMinions > 0) {
				string modMinionText = "Uncountable mod minion slots: " + otherMinions + " / " + player.maxMinions;
				Main.spriteBatch.DrawString(Main.fontItemStack, modMinionText, new Vector2(xPosition, yPosition + Main.buffTexture[1].Height), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
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

				Vector2 size = Utils.Size(Main.buffTexture[150]);

				//Vector2 vector2_1 = new Vector2(num9 - 10 - 47 - 47 - 14, num11 + Main.inventoryBackTexture.Height * 0.5f);
				//- 10 - 47 - 47 - 14: x offset from the right edge of the screen
				//+ Main.inventoryBackTexture.Height * 0.5f: y offset so its aligned with an inv slot
				Vector2 drawPos = new Vector2(x - 10 - 47 - 47 - 14, y + Main.inventoryBackTexture.Height * 0.5f);

				Vector2 offset = Config.Instance.Offset;
				offset = new Vector2((offset.X - Config.DefaultX) * Main.screenWidth, (offset.Y - Config.DefaultY) * Main.screenHeight);

				drawPos += offset;
				drawPos.X = Utils.Clamp(drawPos.X, size.X / 2, Main.screenWidth - size.X / 2);
				drawPos.Y = Utils.Clamp(drawPos.Y, size.Y / 2, Main.screenHeight - size.Y / 2);

				spriteBatch.Draw(Main.buffTexture[150], drawPos, null, Color.White, 0.0f, size / 2f, inventoryScale, SpriteEffects.None, 0f);
				Vector2 stringLength = Main.fontMouseText.MeasureString(player.maxMinions.ToString());
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, player.maxMinions.ToString(), drawPos - stringLength * 0.5f * inventoryScale, Color.White, 0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
				if (Utils.CenteredRectangle(drawPos, size).Contains(new Point(Main.mouseX, Main.mouseY))) {
					player.mouseInterface = true;
					string str = "" + Math.Round(player.slotsMinions, 2) + " / " + player.maxMinions + " Minion Slots";
					if (!string.IsNullOrEmpty(str))
						Main.hoverItemName = str;
				}
			}
		}

		public override object Call(params object[] args) {
			/*
			 * int, int, List<int>, List<float>
			 * or
			 * int, int, List<int>
			 * or
			 * int, int, int, float
			 * or
			 * int, int, int
			 */
			try {
				int itemID = Convert.ToInt32(args[0]);
				int buffID = Convert.ToInt32(args[1]);

				if (itemID >= ItemLoader.ItemCount) throw new Exception("Invalid item registered");

				string itemMsg = " ### Minion from item '" + ItemID.GetUniqueKey(itemID) + "' not added";

				if (buffID >= BuffLoader.BuffCount) throw new Exception("Invalid buff registered" + itemMsg);

				if (args[2] is List<int>) {
					var projIDs = args[2] as List<int>;
					if (projIDs.Count == 0) throw new Exception("Projectile list empty" + itemMsg);
					foreach (int type in projIDs) {
						if (type >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile registered" + itemMsg);
					}
					if (args.Length == 4) {
						var slots = args[3] as List<float>;
						if (projIDs.Count != slots.Count)
							throw new Exception("Length of the projectile list does not match up with the length of the slot list" + itemMsg);
						AddMinion(new MinionModel(itemID, buffID, projIDs, slots));
					}
					else {
						AddMinion(new MinionModel(itemID, buffID, projIDs));
					}
				}
				else {
					int projID = Convert.ToInt32(args[2]);
					if (projID >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile registered" + itemMsg);
					if (args.Length == 4) {
						float slot = Convert.ToSingle(args[3]);
						AddMinion(new MinionModel(itemID, buffID, projID, slot));
					}
					else {
						AddMinion(new MinionModel(itemID, buffID, projID));
					}
				}

				return "Success";
			}
			catch (Exception e) {
				Logger.Error(Name + " Call Error: " + e.StackTrace + e.Message);
			}
			return "Failure";
		}

		/// <summary>
		/// Almost the same as Add, but merges projectile lists on the same buff and registers its item without creating a new model
		/// </summary>
		private void AddMinion(MinionModel model) {
			MinionModel existing = SupportedMinions.SingleOrDefault(m => m.BuffID == model.BuffID);
			if (existing != null) {
				if (!SupportedMinions.Exists(m => m.ItemID == model.ItemID)) {
					ModdedSummonerWeaponsWithExistingBuff.Add(model.ItemID);
				}
				for (int i = 0; i < model.ProjectileIDs.Count; i++) {
					if (!existing.ProjectileIDs.Contains(model.ProjectileIDs[i])) {
						existing.ProjectileIDs.Add(model.ProjectileIDs[i]);
						existing.Slots.Add(model.Slots[i]);
					}
				}
			}
			else {
				SupportedMinions.Add(model);
			}
		}
	}
}
