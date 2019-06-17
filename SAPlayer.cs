using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using SummonersAssociation.Models;
using SummonersAssociation.Items;
using SummonersAssociation.UI;
using Terraria.ID;
using System;

namespace SummonersAssociation
{
	class SAPlayer : ModPlayer
	{
		internal int originalSelectedItem;
		internal bool autoRevertSelectedItem = false;

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

		private bool mouseLeftReleased;

		private bool mouseRightPressed;

		private bool mouseRightReleased;

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
			//unused yet, this is mostly for the history later
			//Not efficient to call it once per item later, need better method, maybe with a passed array of types, and then a single loop, and a scheduler for used items
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
				//Used for third approach
				bool triggerDelete = mouseLeftPressed;
				bool triggerInc = mouseLeftPressed;
				bool triggerDec = mouseRightPressed;

				if (triggerStart && AllowedToOpenHistoryBookUI) {
					bool success = HistoryBookUI.Start();
					if (!success) Main.NewText("Couldn't find any summon weapons in inventory");
				}
				else if (HistoryBookUI.visible) {
					if (HistoryBookUI.heldItemIndex == Main.LocalPlayer.selectedItem) {
						//Keep it updated
						//should this substract from currently summoned minions aswell?
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
						//if in a segment
						else if (HistoryBookUI.IsMouseWithinAnySegment) {
							ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
							if (selected.Active) {
								if (triggerInc) {
									selected.SummonCount = (byte)((selected.SummonCount + 1) % (HistoryBookUI.summonCountTotal + 1));
									try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
									catch { /*No idea why but this threw errors one time*/ }
								}
								else if (triggerDec) {
									selected.SummonCount = (byte)((selected.SummonCount - 1) % (HistoryBookUI.summonCountTotal + 1));
									if (selected.SummonCount == byte.MaxValue) selected.SummonCount = (byte)HistoryBookUI.summonCountTotal;
									try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
									catch { /*No idea why but this threw errors one time*/ }
								}
							}
						}
						//outside the UI
						else {
							if (triggerStop) {
								HistoryBookUI.Stop();
							}
						}
					}
					else {
						//cancel the UI when you switch items
						HistoryBookUI.Stop();
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

			UpdateHistoryBookUI();
		}
	}
}
