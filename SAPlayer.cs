using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using SummonersAssociation.Models;
using SummonersAssociation.Items;
using SummonersAssociation.UI;
using Terraria.ID;

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

			//TODO Change the trigger type here
			bool triggerStart = mouseRightPressed;
			bool triggerStop = mouseRightPressed; //or mouseRightReleased

			if (triggerStart && AllowedToOpenHistoryBookUI) {
				bool success = HistoryBookUI.Start();
				if (!success) Main.NewText("Couldn't find any summon weapons in inventory");
			}
			else if (HistoryBookUI.visible && triggerStop) {
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
		}
	}
}
