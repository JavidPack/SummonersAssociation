using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using SummonersAssociation.Models;
using SummonersAssociation.Items;
using SummonersAssociation.UI;
using System.Collections.Generic;
using System;

namespace SummonersAssociation
{
	class SummonersAssociationPlayer : ModPlayer
	{
		public bool SummonersAssociationCardInInventory = false;
		internal int originalSelectedItem;
		internal bool autoRevertSelectedItem = false;

		internal Queue<Tuple<int, int>> pendingCasts = new Queue<Tuple<int, int>>();

		/// <summary>
		/// Checks if player can open HistoryBookUI
		/// </summary>
		private bool AllowedToOpenHistoryBookUI;

		private bool mouseLeftPressed;

		private bool mouseRightPressed;

		/// <summary>
		/// This is so when the UI is open, it closes itself when player presses ESC (open inventory).
		/// Feels more intuitive if the player can't figure out how to close it otherwise
		/// </summary>
		private bool justOpenedInventory;

		private void UseAutomaticHistoryBook() => QuickUseItemOfType(SummonersAssociation.BookTypes[2]);

		private void UpdateHistoryBookUI() {
			//Since this is UI related, make sure to only run on client
			if (Main.myPlayer == player.whoAmI) {
				//Change the trigger type here
				//Any of the four: mouse(Left/Right)(Pressed/Released)
				bool triggerStart = mouseRightPressed;
				bool triggerStop = mouseRightPressed;
				bool triggerDelete = mouseLeftPressed;
				bool triggerSelect = mouseLeftPressed || mouseRightPressed;
				bool triggerInc = PlayerInput.ScrollWheelDelta > 0 || mouseLeftPressed;
				bool triggerDec = PlayerInput.ScrollWheelDelta < 0 || mouseRightPressed;

				bool holdingBook = Array.IndexOf(SummonersAssociation.BookTypes, player.HeldItem.type) > -1;

				if (triggerStart && holdingBook && AllowedToOpenHistoryBookUI) {
					bool success = HistoryBookUI.Start();
					if (!success) CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.DamagedFriendly, "No summon weapons found");
				}
				else if (HistoryBookUI.visible) {
					if (HistoryBookUI.heldItemIndex == Main.LocalPlayer.selectedItem) {
						//Keep it updated
						HistoryBookUI.summonCountTotal = player.maxMinions;

						if (HistoryBookUI.middle) {
							if (!HistoryBookUI.simple) {
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
										HistoryBookUI.UpdateHistoryBook((MinionHistoryBookSimple)Main.LocalPlayer.HeldItem.modItem);

										HistoryBookUI.Stop();
										CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Saved history");
									}
									else if (HistoryBookUI.returned == HistoryBookUI.NONE) {
										//Nothing, just close
										HistoryBookUI.Stop();
									}
								}
							}
							//If simple, just stop, since middle has no function there (saving is done when clicked on a weapon instead)
							else {
								if (mouseRightPressed || mouseLeftPressed) {
									HistoryBookUI.Stop(false);
								}
							}
						}
						//If in a segment
						else if (HistoryBookUI.IsMouseWithinAnySegment) {
							ItemModel highlighted = HistoryBookUI.itemModels[HistoryBookUI.returned];
							if (highlighted.Active) {
								if (!HistoryBookUI.simple) {
									bool triggered = false;

									if (triggerInc) {
										PlayerInput.ScrollWheelDelta = 0;
										//Only allow to increase if total summon count differential is above or
										//equal to the number of slots needed to summon
										if (HistoryBookUI.summonCountDelta >= highlighted.SlotsNeeded) {
											triggered = true;

											highlighted.SummonCount++;
										}
										else {
											//Indicate that incrementing isn't possible
											HistoryBookUI.colorFadeIn = 1f;
										}
									}
									else if (triggerDec) {
										PlayerInput.ScrollWheelDelta = 0;
										//Only allow to decrease if current summon count is above zero
										if (highlighted.SummonCount > 0) {
											triggered = true;
											highlighted.SummonCount--;
										}
									}

									if (triggered) {
										try { Main.PlaySound(12); }
										catch { /*No idea why but this threw errors one time*/ }
									}
								}
								//If simple, set selected and stop/spawn
								else {
									if (triggerSelect) {
										HistoryBookUI.selected = HistoryBookUI.returned;
										HistoryBookUI.UpdateHistoryBook((MinionHistoryBookSimple)Main.LocalPlayer.HeldItem.modItem);
										CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Selected: " + highlighted.Name);
										HistoryBookUI.Stop();
									}
								}
							}
							else if (HistoryBookUI.simple) {
								if (triggerSelect) {
									//If the selected item is saved in the history but it's not in the players inventory, just stop
									HistoryBookUI.Stop();
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

					if (justOpenedInventory) {
						//Cancel UI when inventory is opened
						HistoryBookUI.Stop(false);
					}
				}
			}
		}

		/// <summary>
		/// Uses the item in the specified index from the players inventory
		/// </summary>
		public void QuickUseItemInSlot(int index) {
			if (index > -1 && index < Main.maxInventory && player.inventory[index].type != 0) {
				originalSelectedItem = player.selectedItem;
				autoRevertSelectedItem = true;
				player.selectedItem = index;
				player.controlUseItem = true;
				player.ItemCheck(Main.myPlayer);
			}
		}

		/// <summary>
		/// Uses the first found item of the specified type from the players inventory
		/// </summary>
		public void QuickUseItemOfType(int type) {
			//Prefer to use this when spawning from an ItemModel because InventoryIndex won't be accurate after the UI is closed and the inventory is modified
			if (type > 0) {
				for (int i = 0; i < Main.maxInventory; i++) {
					Item item = player.inventory[i];
					if (item.type == type) {
						QuickUseItemInSlot(i);
						return;
					}
				}
			}
		}

		public override void ResetEffects() => SummonersAssociationCardInInventory = false;

		public override void ProcessTriggers(TriggersSet triggersSet) {
			//In here things like Main.MouseScreen are not correct (related to UI)
			//and in UpdateUI Main.mouseRight etc aren't correct
			//but you need both to properly interact with the UI
			//these two are used in PreUpdate, together with AllowedToOpenHistoryBookUI
			//since that also doesn't work in ProcessTriggers (set in PostUpdate)

			mouseLeftPressed = Main.mouseLeft && Main.mouseLeftRelease;

			mouseRightPressed = Main.mouseRight && Main.mouseRightRelease;

			justOpenedInventory = PlayerInput.Triggers.JustPressed.Inventory && !Main.playerInventory;
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

		public override void PostUpdate() {
			//This has to be set in PostUpdate cause crucial fields in PreUpdate are not set correctly
			//(representative of the fields)
			AllowedToOpenHistoryBookUI =
				!HistoryBookUI.visible &&
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
		}

		public override void OnRespawn(Player player) => UseAutomaticHistoryBook();

		public override void OnEnterWorld(Player player) => UseAutomaticHistoryBook();
	}
}
