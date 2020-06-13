using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using SummonersAssociation.Items;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using System.IO;
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
		private bool SupportedMinionsFinalized = false;

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

		public override void Load() {
			Instance = this;

			if (!Main.dedServ && Main.netMode != NetmodeID.Server) {
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

			MinionControlRod.LoadHooks();
		}

		public override void PostSetupContent()
			//don't change order here (simple is first, normal is second, automatic is third)
			=> BookTypes = new int[] {
				ModContent.ItemType<MinionHistoryBookSimple>(),
				ModContent.ItemType<MinionHistoryBook>(),
				ModContent.ItemType<MinionHistoryBookAuto>()
			};

		public override void Unload() {
			HistoryBookUIInterface = null;
			HistoryBookUI = null;
			HistoryBookUI.redCrossTexture = null;

			SupportedMinions = null;
			BookTypes = null;

			MinionControlRod.UnloadHooks();

			Instance = null;
		}

		public override void AddRecipeGroups() {
			// Automatically register MinionModels
			var item = new Item();
			var projectile = new Projectile();
			for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
				item = ItemLoader.GetItem(i).item;
				if(item.buffType > 0 && item.shoot >= ProjectileID.Count) {
					projectile = ProjectileLoader.GetProjectile(item.shoot).projectile;
					if (projectile.minionSlots > 0) {
						// Avoid automatic support for manually supported
						if (!SupportedMinions.Any(x => x.ItemID == i || x.ProjectileIDs.Contains(projectile.type) || x.BuffID == item.buffType)) {
							AddMinion(new MinionModel(item.type, item.buffType, projectile.type));
						}
					}
				}
			}
			SupportedMinionsFinalized = true;

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
					lineOffset = b / buffsPerLine;
					int buffID = player.buffType[b];
					xPosition = 32 + (b - lineOffset * buffsPerLine) * 38;
					yPosition = 76 + lineOffset * 50 + Main.buffTexture[buffID].Height;
					color = new Color(new Vector4(Main.buffAlpha[b]));

					int number = 0;
					double slots = 0;

					// Check to see if this buff represents a minion or not
					MinionModel minion = SupportedMinions.SingleOrDefault(minionEntry => minionEntry.BuffID == buffID);
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
						spriteBatch.DrawString(Main.fontItemStack, ratio, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
					}
				}
			}
			//Count non-registered mod minions
			color = new Color(new Vector4(0.4f));
			xPosition = 32;
			yPosition = 76 + 20 + lineOffset * 50 + Main.buffTexture[1].Height;
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
				Main.spriteBatch.DrawString(Main.fontItemStack, modMinionText, new Vector2(xPosition, yPosition), color, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
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


		//Examples:
		//############
		//Mod summonersAssociation = ModLoader.GetMod("SummonersAssociation");
		//
		//	Regular call for a regular summon weapon
		//	summonersAssociation?.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		ProjectileType<MinionProjectile>()
		//	);
		//
		//  If the weapon summons two minions
		//	summonersAssociation?.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new List<int> {
		//		ProjectileType<MinionProjectile1>(),
		//		ProjectileType<MinionProjectile2>()
		//		}
		//	);
		//
		//  If you want to override the minionSlots of the projectile
		//  (for example useful if you have a Stardust Dragon-like minion and you only want it to count one segment towards the number of summoned minions)
		//	summonersAssociation?.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		ProjectileType<MinionProjectile>(),
		//		1f
		//	);
		//
		//  If you want to override the minionSlots of the projectiles you specify (same order)
		//  (for example useful if you have some complex minion that consists of multiple parts)
		//	summonersAssociation?.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new List<int> {
		//		ProjectileType<MinionProjectile1>(),
		//		ProjectileType<MinionProjectile2>()
		//		},
		//		new List<float> {
		//		0.25f,
		//		0.5f
		//		}
		//	);

		public override object Call(params object[] args) {
			/* message string, then 
			 * if "AddMinionInfo":
			 * int, int, List<int>, List<float>
			 * or
			 * int, int, List<int>
			 * or
			 * int, int, int, float
			 * or
			 * int, int, int
			 * 
			 * else if "SomeOther":
			 * ...
			 */
			try {
				string message = args[0] as string;
				if (message == "AddMinionInfo") {
					if (SupportedMinionsFinalized)
						throw new Exception($"{Name} Call Error: The attempted message, \"{message}\", was sent too late. {Name} expects Call messages to happen during Mod.PostSetupContent.");

					int itemID = Convert.ToInt32(args[1]);
					int buffID = Convert.ToInt32(args[2]);

					if (itemID == 0 || itemID >= ItemLoader.ItemCount) throw new Exception("Invalid item '" + itemID + "' registered");

					string itemMsg = " ### Minion from item '" + ItemID.GetUniqueKey(itemID) + "' not added";

					if (buffID == 0 || buffID >= BuffLoader.BuffCount) throw new Exception("Invalid buff '" + buffID + "' registered" + itemMsg);

					if (args[3] is List<int>) {
						var projIDs = args[3] as List<int>;
						if (projIDs.Count == 0) throw new Exception("Projectile list empty" + itemMsg);
						foreach (int type in projIDs) {
							if (type == 0 || type >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile '" + type + "' registered" + itemMsg);
						}
						if (args.Length == 5) {
							var slots = args[4] as List<float>;
							if (projIDs.Count != slots.Count)
								throw new Exception("Length of the projectile list does not match up with the length of the slot list" + itemMsg);
							AddMinion(new MinionModel(itemID, buffID, projIDs, slots));
						}
						else {
							AddMinion(new MinionModel(itemID, buffID, projIDs));
						}
					}
					else {
						int projID = Convert.ToInt32(args[3]);
						if (projID == 0 || projID >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile '" + projID + "' registered" + itemMsg);
						if (args.Length == 5) {
							float slot = Convert.ToSingle(args[4]);
							AddMinion(new MinionModel(itemID, buffID, projID, slot));
						}
						else {
							AddMinion(new MinionModel(itemID, buffID, projID));
						}
					}
					return "Success";
				}
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
					int id = model.ProjectileIDs[i];
					if (!existing.ProjectileIDs.Contains(id)) {
						existing.ProjectileIDs.Add(id);
						existing.Slots.Add(model.Slots[i]);
					}
				}
			}
			else {
				SupportedMinions.Add(model);
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();

			if (Enum.IsDefined(typeof(PacketType), type)) {
				var packetType = (PacketType)type;
				switch (packetType) {
					case PacketType.SpawnTarget:
						MinionControlRod.HandleSpawnTarget(reader);
						break;
					case PacketType.ConfirmTargetToClient:
						MinionControlRod.HandleConfirmTargetToClient(reader);
						break;
					default:
						Logger.Warn("'None' packet type received");
						break;
				}
			}
			else {
				Logger.Warn("Undefined packet type received: " + type);
			}
		}
	}
}
