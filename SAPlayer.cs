using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using SummonersAssociation.Models;
using SummonersAssociation.Items;
using SummonersAssociation.UI;
using Terraria.ID;
using System.Collections.Generic;
using System;

namespace SummonersAssociation
{
	class SAPlayer : ModPlayer
	{
		internal int originalSelectedItem;
		internal bool autoRevertSelectedItem = false;
		internal Queue<Tuple<int, int>> pendingCasts = new Queue<Tuple<int, int>>();

		/// <summary>
		/// Checks if player can open HistoryBookUI
		/// </summary>
		private bool AllowedToOpenHistoryBookUI =>
			!HistoryBookUI.visible &&
			player.HeldItem.type == mod.ItemType<MinionHistoryBook>() &&
			Main.hasFocus &&
			!Main.gamePaused &&
			!player.dead &&
			!player.mouseInterface &&
			!Main.drawingPlayerChat &&
			!Main.editSign &&
			!Main.editChest &&
			!Main.blockInput &&
			!Main.mapFullscreen &&
			!Main.HoveringOverAnNPC &&
			!player.showItemIcon &&
			player.talkNPC == -1 &&
			player.itemTime == 0 && player.itemAnimation == 0 &&
			!(player.frozen || player.webbed || player.stoned);

		private bool mouseLeftPressed;

		private bool mouseLeftReleased; //Unused

		private bool mouseRightPressed;

		private bool mouseRightReleased; //Unused

		/// <summary>
		/// Uses the item in the specified index from the players inventory
		/// </summary>
		public void QuickUseItemInSlot(int index) {
			if (index > -1 && index < Main.maxInventory && player.inventory[index].type != 0) {
				if (!player.inventory[index].summon) return; //safety check
				originalSelectedItem = player.selectedItem;
				autoRevertSelectedItem = true;
				Main.LocalPlayer.selectedItem = index;
				Main.LocalPlayer.controlUseItem = true;
				Main.LocalPlayer.ItemCheck(Main.myPlayer);
			}
		}

		/// <summary>
		/// Uses the first found item of the specified type from the players inventory
		/// </summary>
		public void QuickUseItemOfType(int type) {
			//Prefer to use this when spawning from an ItemModel because InventoryIndex won't be accurate after the UI is closed and the inventory is modified
			//Unused yet, this is mostly for the history later
			//Need better method, maybe with a passed array of types, and then a single loop, and a scheduler for used items
			if (type > 0) {
				for (int i = 0; i < Main.maxInventory; i++) {
					Item item = Main.LocalPlayer.inventory[i];
					if (item.type == type) {
						QuickUseItemInSlot(i);
						return;
					}
				}
			}
		}

		private void UpdateHistoryBookUI() {
			//Since this is UI related, make sure to only run on client
			if (Main.myPlayer == player.whoAmI) {
				//Change the trigger type here
				//Any of the four: mouse(Left/Right)(Pressed/Released)
				bool triggerStart = mouseRightPressed;
				bool triggerStop = mouseRightPressed;
				bool triggerDelete = mouseLeftPressed;
				bool triggerInc = PlayerInput.ScrollWheelDelta > 0 || mouseLeftPressed;
				bool triggerDec = PlayerInput.ScrollWheelDelta < 0 || mouseRightPressed;

				if (triggerStart && AllowedToOpenHistoryBookUI) {
					bool success = HistoryBookUI.Start();
					if (!success) Main.NewText("Couldn't find any summon weapons in inventory");
				}
				else if (HistoryBookUI.visible) {
					if (HistoryBookUI.heldItemIndex == Main.LocalPlayer.selectedItem) {
						//Keep it updated
						//Should this substract from currently summoned minions aswell?
						HistoryBookUI.summonCountTotal = player.maxMinions /*- (int)Math.Round(player.slotsMinions)*/;

						if (HistoryBookUI.middle) {
							if (triggerDelete) {
								if (!HistoryBookUI.aboutToDelete) {
									HistoryBookUI.aboutToDelete = true;
								}
								else {
									//Clear history, and use fresh inventory data
									HistoryBookUI.itemModels = HistoryBookUI.GetSummonWeapons();
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Reset history");
									HistoryBookUI.aboutToDelete = false;
								}
							}
							else if (triggerStop) {
								if (HistoryBookUI.aboutToDelete) {
									HistoryBookUI.aboutToDelete = false;
								}
								else if (HistoryBookUI.returned == HistoryBookUI.UPDATE) {
									HistoryBookUI.UpdateHistoryBook((MinionHistoryBook)Main.LocalPlayer.HeldItem.modItem);

									HistoryBookUI.Stop();
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Saved history");
								}
								else if (HistoryBookUI.returned == HistoryBookUI.NONE) {
									//Nothing, just close
									HistoryBookUI.Stop();
								}
							}
						}
						//If in a segment
						else if (HistoryBookUI.IsMouseWithinAnySegment) {
							ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
							if (selected.Active) {
								bool triggered = false;
								if (triggerInc) {
									PlayerInput.ScrollWheelDelta = 0;
									//Only allow to increase if total summon count differential is above zero
									if (HistoryBookUI.summonCountDelta > 0) {
										triggered = true;
										selected.SummonCount = (byte)((selected.SummonCount + 1) % (HistoryBookUI.summonCountTotal + 1));
									}
								}
								else if (triggerDec) {
									PlayerInput.ScrollWheelDelta = 0;
									//Only allow to decrease if current sumon count is above zero
									if (selected.SummonCount > 0) {
										triggered = true;
										selected.SummonCount = (byte)((selected.SummonCount - 1) % (HistoryBookUI.summonCountTotal + 1));
									}
								}

								if (triggered) {
									try { Main.PlaySound(12); }
									catch { /*No idea why but this threw errors one time*/ }
								}
							}
						}
						//Outside the UI
						else {
							/*if (triggerStop) {*/
							if (mouseRightPressed || mouseLeftPressed) {
								HistoryBookUI.Stop();
							}
						}
					}
					else {
						//Cancel the UI when you switch items
						HistoryBookUI.Stop(false);
					}
				}
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			//In here things like Main.MouseScreen are not correct (related to UI)
			//and in UpdateUI Main.mouseRight etc aren't correct
			//but you need both to properly open/close the UI
			//these two are used in PreUpdate(), together with AllowedToOpenHistoryBookUI
			//since that also doesn't work in ProcessTriggers

			mouseLeftPressed = Main.mouseLeft && Main.mouseLeftRelease;

			mouseLeftReleased = !Main.mouseLeft && !Main.mouseLeftRelease;

			mouseRightPressed = Main.mouseRight && Main.mouseRightRelease;

			mouseRightReleased = !Main.mouseRight && !Main.mouseRightRelease;
		}

		public override void PreUpdate() {
			if (autoRevertSelectedItem) {
				if (player.itemTime == 0 && player.itemAnimation == 0) {
					player.selectedItem = originalSelectedItem;
					autoRevertSelectedItem = false;
				}
			}

			if (player.itemTime == 0 && player.itemAnimation == 0) {
				if (pendingCasts.Count > 0) {
					var cast = pendingCasts.Dequeue();
					QuickUseItemOfType(cast.Item1);
				}
			}

			UpdateHistoryBookUI();
		}
	}
}
