using SummonersAssociation.Items;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using System.IO;

namespace SummonersAssociation
{
	class SummonersAssociationPlayer : ModPlayer
	{
		public bool SummonersAssociationCardInInventory = false;
		internal int originalSelectedItem;
		internal bool autoRevertSelectedItem = false;

		internal bool enteredWorld = false; //Because OnEnterWorld is unreliable

		internal double lastOtherMinions = 0;

		internal Queue<Tuple<int, int>> pendingCasts = new Queue<Tuple<int, int>>();

		/// <summary>
		/// true during item use when <see cref="pendingCasts"/>.Count > 0. Clientside
		/// </summary>
		private bool speedUpItemUse = false;

		/// <summary>
		/// Checks if player can open LoadoutBookUI
		/// </summary>
		private bool AllowedToOpenLoadoutBookUI;

		/// <summary>
		/// This is so when the UI is open, it closes itself when player presses ESC (open inventory).
		/// Feels more intuitive if the player can't figure out how to close it otherwise
		/// </summary>
		private bool justOpenedInventory;

		private bool mouseLeftPressed;

		private bool mouseRightPressed;

		//Any of the four: mouse(Left/Right)(Pressed/Released)
		private bool TriggerStart => mouseRightPressed;

		private bool TriggerStop => mouseRightPressed;

		private bool TriggerDelete => mouseLeftPressed;

		private bool TriggerSelect => mouseLeftPressed || mouseRightPressed;

		private bool TriggerInc => PlayerInput.ScrollWheelDelta > 0 || mouseLeftPressed;

		private bool TriggerDec => PlayerInput.ScrollWheelDelta < 0 || mouseRightPressed;

		public int TargetWhoAmI { get; set; } = -1; //Only relevant for server, and the client "owning" it

		public bool HasValidTarget => TargetWhoAmI > -1 && TargetWhoAmI < Main.maxNPCs;

		public NPC Target => HasValidTarget ? Main.npc[TargetWhoAmI] : null;

		public int PendingTargetAssignment { get; set; } = -1;

		private void UseAutomaticLoadoutBook() {
			int slot = -1;
			for (int i = 0; i < Main.InventorySlotsTotal; i++) {
				Item item = Player.inventory[i];
				if (item.type == SummonersAssociation.BookTypes[2]) {
					var book = (MinionLoadoutBookSimple)item.ModItem;
					if (book.loadout.Sum(x => x.Active ? x.SummonCount : 0) > 0) slot = i;
				}
			}
			if (slot != -1) QuickUseItemInSlot(slot);
		}

		private void UpdateLoadoutBookUI() {
			//Since this is UI related, make sure to only run on client

			bool holdingBook = Array.IndexOf(SummonersAssociation.BookTypes, Player.HeldItem.type) > -1;

			if (TriggerStart && holdingBook && AllowedToOpenLoadoutBookUI) {
				bool success = LoadoutBookUI.Start();
				if (!success) CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.DamagedFriendly, UISystem.LoadoutBookOnUseNoWeapons.ToString());
				//if (success) Main.NewText("" + Main.time + ": just opened the UI");
			}
			else if (LoadoutBookUI.visible) {
				LoadoutBookUI.visible = false;
				if (LoadoutBookUI.heldItemIndex == Main.LocalPlayer.selectedItem) {
					//Keep it updated
					LoadoutBookUI.summonCountTotal = Player.maxMinions;

					if (LoadoutBookUI.middle) {
						if (!LoadoutBookUI.simple) {
							if (TriggerDelete) {
								if (!LoadoutBookUI.aboutToDelete) {
									LoadoutBookUI.aboutToDelete = true;
								}
								else {
									//Clear loadout, and use fresh inventory data
									LoadoutBookUI.itemModels = LoadoutBookUI.GetSummonWeapons();
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, UISystem.LoadoutBookOnUseReset.ToString());
									LoadoutBookUI.aboutToDelete = false;
								}
							}
							else if (TriggerStop) {
								if (LoadoutBookUI.aboutToDelete) {
									LoadoutBookUI.aboutToDelete = false;
								}
								else if (LoadoutBookUI.returned == LoadoutBookUI.UPDATE) {
									LoadoutBookUI.UpdateLoadoutBook((MinionLoadoutBookSimple)Player.HeldItem.ModItem);

									LoadoutBookUI.Stop();
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, UISystem.LoadoutBookOnUseSaved.ToString());
									//Main.NewText("" + Main.time + ": just saved loadout");
									//Main.NewText("######");
								}
								else if (LoadoutBookUI.returned == LoadoutBookUI.NONE) {
									//Nothing, just close
									LoadoutBookUI.Stop();
								}
							}
						}
						//If simple, just stop, since middle has no function there (saving is done when clicked on a weapon instead)
						else {
							if (mouseRightPressed || mouseLeftPressed) {
								LoadoutBookUI.Stop(false);
							}
						}
					}
					//If in a segment
					else if (LoadoutBookUI.IsMouseWithinAnySegment) {
						ItemModel highlighted = LoadoutBookUI.itemModels[LoadoutBookUI.returned];
						if (highlighted.Active) {
							if (!LoadoutBookUI.simple) {
								bool triggered = false;

								if (TriggerInc) {
									PlayerInput.ScrollWheelDelta = 0;
									//Only allow to increase if total summon count differential is above or
									//equal to the number of slots needed to summon
									if (LoadoutBookUI.summonCountDelta >= highlighted.SlotsFilledPerUse) {
										triggered = true;

										highlighted.SummonCount++;
									}
									else {
										//Indicate that incrementing isn't possible
										LoadoutBookUI.colorFadeIn = 1f;
									}
								}
								else if (TriggerDec) {
									PlayerInput.ScrollWheelDelta = 0;
									//Only allow to decrease if current summon count is above zero
									if (highlighted.SummonCount > 0) {
										triggered = true;
										highlighted.SummonCount--;
									}
								}

								if (triggered) {
									try { SoundEngine.PlaySound(SoundID.MenuTick); }
									catch { /*No idea why but this threw errors one time*/ }
								}
							}
							//If simple, set selected and stop/spawn
							else {
								if (TriggerSelect) {
									LoadoutBookUI.selected = LoadoutBookUI.returned;
									LoadoutBookUI.UpdateLoadoutBook((MinionLoadoutBookSimple)Main.LocalPlayer.HeldItem.ModItem);
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, UISystem.LoadoutBookOnUseSelected.Format(highlighted.Name));
									LoadoutBookUI.Stop();
								}
							}
						}
						else if (LoadoutBookUI.simple) {
							if (TriggerSelect) {
								//If the selected item is saved in the loadout but it's not in the players inventory, just stop
								LoadoutBookUI.Stop();
							}
						}
						PlayerInput.ScrollWheelDelta = 0;
					}
					//Outside the UI
					else {
						/*if (triggerStop) {*/
						if (mouseRightPressed || mouseLeftPressed) {
							LoadoutBookUI.Stop();
						}
					}
				}
				else {
					//Cancel the UI when you switch items
					LoadoutBookUI.Stop(false);
				}

				if (justOpenedInventory) {
					//Cancel UI when inventory is opened
					LoadoutBookUI.Stop(false);
				}
			}
		}

		/// <summary>
		/// Uses the item in the specified index from the players inventory
		/// </summary>
		public void QuickUseItemInSlot(int index) {
			if (index > -1 && index < Main.InventorySlotsTotal && Player.inventory[index].type != ItemID.None) {
				if (Player.CheckMana(Player.inventory[index], -1, false, false)) {
					originalSelectedItem = Player.selectedItem;
					autoRevertSelectedItem = true;
					Player.selectedItem = index;
					Player.controlUseItem = true;
					//if(Main.netMode == 1)
					//	NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
					// TODO: Swings not shown on other clients.
					Player.ItemCheck();
				}
				else
					SoundEngine.PlaySound(SoundID.Drip with { Variants = stackalloc int[] { 0, 1, 2 } }, Player.Center);
			}
		}

		/// <summary>
		/// Uses the first found item of the specified type from the players inventory
		/// </summary>
		public void QuickUseItemOfType(int type) {
			//Prefer to use this when spawning from an ItemModel because InventoryIndex won't be accurate after the UI is closed and the inventory is modified
			if (type > 0) {
				for (int i = 0; i < Main.InventorySlotsTotal; i++) {
					Item item = Player.inventory[i];
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
			//these two are used in PreUpdate, together with AllowedToOpenLoadoutBookUI
			//since that also doesn't work in ProcessTriggers (set in PostUpdate)

			mouseLeftPressed = Main.mouseLeft && Main.mouseLeftRelease;

			mouseRightPressed = Main.mouseRight && Main.mouseRightRelease;

			justOpenedInventory = PlayerInput.Triggers.JustPressed.Inventory && !Main.playerInventory;
		}

		public override float UseSpeedMultiplier(Item item)
		{
			if (speedUpItemUse) {
				float speed = ServerConfig.Instance.LoadoutBookSpeed;
				if (speed == ServerConfig.LoadoutBookSpeed_Max)
					speed = 10000; //Make it "instant"

				return speed;
			}

			return base.UseSpeedMultiplier(item);
		}

		public override void PreUpdate() {
			if (Player.whoAmI == Main.myPlayer) {
				UpdateLoadoutBookUI();
			}

			//Player.ItemTimeIsZero checks don't work on multiplayer properly, the code works without them
			if (autoRevertSelectedItem) {
				if (Player.ItemAnimationEndingOrEnded /*&& Player.ItemTimeIsZero*/) {
					Player.selectedItem = originalSelectedItem;
					autoRevertSelectedItem = false;
				}
			}

			if (Player.ItemAnimationEndingOrEnded /*&& Player.ItemTimeIsZero*/) {
				if (pendingCasts.Count > 0) {
					speedUpItemUse = true;
					var cast = pendingCasts.Dequeue();
					QuickUseItemOfType(cast.Item1);
				}
				else {
					speedUpItemUse = false;
				}
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)PacketType.SyncPlayer);
			packet.Write((byte)Player.whoAmI);
			packet.Write((bool)enteredWorld);
			packet.Send(toWho, fromWho);
		}

		public static void ReceiveSyncPlayer(BinaryReader reader) {
			byte whoAmI = reader.ReadByte();
			Player player = Main.player[whoAmI];
			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
			mPlayer.enteredWorld = reader.ReadBoolean();
		}

		public override void PostUpdate() {
			if (!enteredWorld) {
				enteredWorld = true;
				UseAutomaticLoadoutBook();
			}

			MinionControlRod.PendingTargetAssignment(this);
			MinionControlRod.TargetVerification(this);

			//This has to be set in PostUpdate cause crucial fields in PreUpdate are not set correctly
			//(representative of the fields)
			AllowedToOpenLoadoutBookUI =
				!LoadoutBookUI.visible &&
				Main.hasFocus &&
				!Main.gamePaused &&
				!Player.dead &&
				!Player.mouseInterface &&
				!Main.drawingPlayerChat &&
				!Main.editSign &&
				!Main.editChest &&
				!Main.blockInput &&
				!Main.mapFullscreen &&
				!Main.HoveringOverAnNPC &&
				!Player.cursorItemIconEnabled &&
				Player.talkNPC == -1 &&
				Player.itemTime == 0 && Player.itemAnimation == 0 &&
				!Player.CCed;
		}

		public override void OnRespawn() => UseAutomaticLoadoutBook();
	}
}
