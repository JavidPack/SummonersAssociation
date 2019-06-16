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
		internal bool canOpenUI = false;
		public bool justspawned = false;

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
			if (index > HistoryBookUI.NONE && index < player.inventory.Length && player.inventory[index].type != 0) {
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
				for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++) {
					Item item = Main.LocalPlayer.inventory[i];
					if (item.type == type) {
						QuickUseItemInSlot(i);
						return;
					}
				}
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			//In here things like Main.MouseScreen are not correct (related to UI)
			//and in UpdateUI Main.mouseRight etc aren't correct
			//but you need both to properly open/close the UI
			//these two are used in PostUpdate(), together with AllowedToOpenHistoryBookUI
			//since that also doesn't work in ProcessTriggers

			mouseLeftPressed = Main.mouseLeft && Main.mouseLeftRelease;

			mouseLeftReleased = !Main.mouseLeft && !Main.mouseLeftRelease;

			mouseRightPressed = Main.mouseRight && Main.mouseRightRelease;

			mouseRightReleased = !Main.mouseRight && !Main.mouseRightRelease;
		}

		public override void PostUpdate() {
			if (autoRevertSelectedItem) {
				if (player.itemTime == 0 && player.itemAnimation == 0) {
					player.selectedItem = originalSelectedItem;
					autoRevertSelectedItem = false;
				}
			}

			//Since this is UI related, make sure to only run on client
			if (Main.myPlayer == player.whoAmI) {
				Main.NewText(HistoryBookUI.returned);
				//Change the trigger type here
				//any of the four: mouse(Left/Right)(Pressed/Released)
				bool triggerStart = mouseRightPressed;
				bool triggerStop = mouseRightPressed;
				//used for third approach
				bool triggerInc = mouseLeftPressed;
				bool triggerDec = mouseRightPressed;

				if (triggerStart && AllowedToOpenHistoryBookUI) {
					bool success = HistoryBookUI.Start();
					if (!success) Main.NewText("Couldn't find any summon weapons in inventory");
				}
				else if (HistoryBookUI.visible) {
					//first simple approach, exit whereever you clicked (and spawn the selected one)
					//if triggerStop == mouseRightReleased: exit whereever you let go of mouseRight

					//if (triggerStop && HistoryBookUI.returned > HistoryBookUI.NONE) {
					//	//If something returned

					//	try {
					//		Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position);
					//	}
					//	catch {
					//		//No idea why but this threw errors one time
					//	}
					//	ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
					//	CombatText.NewText(player.getRect(), CombatText.HealLife, "Selected: " + selected.Name);

					//	//Update
					//	HistoryBookUI.UpdateHistoryBookItemsInInventory();

					//	//Try to use selected summon item
					//	QuickUseItemInSlot(selected.InventoryIndex);
					//}
					//
					//HistoryBookUI.Stop();

					//second simple approach, only exit when clicked in the middle (and spawn the latest selected one)
					//if (triggerStop && HistoryBookUI.middle) {
					//	if (HistoryBookUI.returned > HistoryBookUI.NONE) {
					//		//If something returned
					//		try {
					//			Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position);
					//		}
					//		catch {
					//			//No idea why but this threw errors one time
					//		}
					//		ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
					//		CombatText.NewText(player.getRect(), CombatText.HealLife, "Selected: " + selected.Name);

					//		//Update
					//		HistoryBookUI.UpdateHistoryBookItemsInInventory();

					//		//Try to use selected summon item
					//		QuickUseItemInSlot(selected.InventoryIndex);
					//	}

					//	HistoryBookUI.Stop();
					//}

					//third more advanced approach, increment/decrement when not clicked in the middle,
					//only exit when clicked in the middle (no logic regarding what to do with the counter yet)
					if (triggerStop && HistoryBookUI.middle) {
						if (HistoryBookUI.returned > HistoryBookUI.NONE) {
							//If something returned
							try {
								Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position);
							}
							catch {
								//No idea why but this threw errors one time
							}
							ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
							CombatText.NewText(player.getRect(), CombatText.HealLife, "Selected: " + selected.Name);

							//Update
							HistoryBookUI.UpdateHistoryBookItemsInInventory();

							//Try to use selected summon item
							QuickUseItemInSlot(selected.InventoryIndex);
						}

						HistoryBookUI.Stop();
					}
					//if in a segment
					else if (HistoryBookUI.isMouseWithinUI && !HistoryBookUI.middle && HistoryBookUI.returned > HistoryBookUI.NONE) {
						ItemModel selected = HistoryBookUI.itemModels[HistoryBookUI.returned];
						if (triggerInc) {
							selected.SummonCount = (byte)((selected.SummonCount + 1) % 11);
						}
						else if (triggerDec) {
							selected.SummonCount = (byte)((selected.SummonCount - 1) % 11);
							if (selected.SummonCount == byte.MaxValue) selected.SummonCount = 10;
						}
					}
				}
			}
		}
	}
}
