using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummonersAssociation.Items;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	class SummonersAssociationPlayer : ModPlayer
	{
		public bool SummonersAssociationCardInInventory = false;
		internal int originalSelectedItem;
		internal bool autoRevertSelectedItem = false;

		internal Queue<Tuple<int, int>> pendingCasts = new Queue<Tuple<int, int>>();
		internal List<int> lastRightClicks = new List<int>();

		/// <summary>
		/// Checks if player can open HistoryBookUI
		/// </summary>
		private bool AllowedToOpenHistoryBookUI;

		/// <summary>
		/// This is so when the UI is open, it closes itself when player presses ESC (open inventory).
		/// Feels more intuitive if the player can't figure out how to close it otherwise
		/// </summary>
		private bool justOpenedInventory;

		private bool mouseLeftPressed;

		private bool mouseRightPressed;
		private bool mouseRightHeld;

		//Any of the four: mouse(Left/Right)(Pressed/Released)
		private bool TriggerStart => mouseRightPressed;

		private bool TriggerStop => mouseRightPressed;

		private bool TriggerDelete => mouseLeftPressed;

		private bool TriggerSelect => mouseLeftPressed || mouseRightPressed;

		private bool TriggerInc => PlayerInput.ScrollWheelDelta > 0 || mouseLeftPressed;

		private bool TriggerDec => PlayerInput.ScrollWheelDelta < 0 || mouseRightPressed;

		private void UseAutomaticHistoryBook() {
			int slot = -1;
			for (int i = 0; i < Main.maxInventory; i++) {
				Item item = player.inventory[i];
				if (item.type == SummonersAssociation.BookTypes[2]) {
					var book = (MinionHistoryBookSimple)item.modItem;
					if (book.history.Sum(x => x.Active ? x.SummonCount : 0) > 0) slot = i;
				}
			}
			if (slot != -1) QuickUseItemInSlot(slot);
		}

		public static readonly PlayerLayer Debug = new PlayerLayer("SummonersAssociation", "Debug", PlayerLayer.MiscEffectsBack, delegate (PlayerDrawInfo drawInfo) {
			Player player = drawInfo.drawPlayer;
			if (drawInfo.shadow != 0f || player.dead || Array.IndexOf(SummonersAssociation.BookTypes, player.HeldItem.type) < 0) {
				return;
			}
			SummonersAssociationPlayer mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();

			float drawX = (int)drawInfo.position.X - Main.screenPosition.X;
			float drawY = (int)drawInfo.position.Y - Main.screenPosition.Y;

			var stupidOffset = new Vector2(player.width, player.height);
			var ogpos = new Vector2(drawX, drawY) + player.bodyPosition + stupidOffset;
			var pos = ogpos;

			string text = "Pressed: ";
			HistoryBookUI.DrawText(Main.spriteBatch, text, pos, Color.White);
			pos.X += Main.fontMouseText.MeasureString(text).X;
			foreach (int pressed in mPlayer.lastRightClicks) {
				text = "" + (int)Math.Round(pressed / 20d);
				Color color = Color.White;
				HistoryBookUI.DrawText(Main.spriteBatch, text, pos, color);
				pos.X += Main.fontMouseText.MeasureString(text).X + 8;
			}
			pos = ogpos;
			pos.Y += 22;
			text = "Held: " + (mPlayer.mouseRightHeld ? "Yes" : "No");
			HistoryBookUI.DrawText(Main.spriteBatch, text, pos, Color.White);
		});

		//public override void ModifyDrawLayers(List<PlayerLayer> layers) => layers.Add(Debug);


		private void UpdateHistoryBookUI() {
			//Since this is UI related, make sure to only run on client
			//Change the trigger type here

			bool holdingBook = Array.IndexOf(SummonersAssociation.BookTypes, player.HeldItem.type) > -1;

			if (TriggerStart && holdingBook && AllowedToOpenHistoryBookUI) {
				bool success = HistoryBookUI.Start();
				if (!success) CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.DamagedFriendly, "No summon weapons found");
				//if (success) Main.NewText("" + Main.time + ": just opened the UI");
			}
			else if (HistoryBookUI.visible) {
				HistoryBookUI.visible = false;
				if (HistoryBookUI.heldItemIndex == Main.LocalPlayer.selectedItem) {
					//Keep it updated
					HistoryBookUI.summonCountTotal = player.maxMinions;

					if (HistoryBookUI.middle) {
						if (!HistoryBookUI.simple) {
							if (TriggerDelete) {
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
							else if (TriggerStop) {
								if (HistoryBookUI.aboutToDelete) {
									HistoryBookUI.aboutToDelete = false;
								}
								else if (HistoryBookUI.returned == HistoryBookUI.UPDATE) {
									HistoryBookUI.UpdateHistoryBook((MinionHistoryBookSimple)player.HeldItem.modItem);

									HistoryBookUI.Stop();
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Saved history");
									//Main.NewText("" + Main.time + ": just saved history");
									//Main.NewText("######");
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

								if (TriggerInc) {
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
								else if (TriggerDec) {
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
								if (TriggerSelect) {
									HistoryBookUI.selected = HistoryBookUI.returned;
									HistoryBookUI.UpdateHistoryBook((MinionHistoryBookSimple)Main.LocalPlayer.HeldItem.modItem);
									CombatText.NewText(Main.LocalPlayer.getRect(), CombatText.HealLife, "Selected: " + highlighted.Name);
									HistoryBookUI.Stop();
								}
							}
						}
						else if (HistoryBookUI.simple) {
							if (TriggerSelect) {
								//If the selected item is saved in the history but it's not in the players inventory, just stop
								HistoryBookUI.Stop();
							}
						}
						PlayerInput.ScrollWheelDelta = 0;
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

		/// <summary>
		/// Uses the item in the specified index from the players inventory
		/// </summary>
		public void QuickUseItemInSlot(int index) {
			if (index > -1 && index < Main.maxInventory && player.inventory[index].type != 0) {
				if (player.CheckMana(player.inventory[index], -1, false, false)) {
					originalSelectedItem = player.selectedItem;
					autoRevertSelectedItem = true;
					player.selectedItem = index;
					player.controlUseItem = true;
					//if(Main.netMode == 1)
					//	NetMessage.SendData(13, -1, -1, null, Main.myPlayer, 0f, 0f, 0f, 0, 0, 0);
					// TODO: Swings not shown on other clients.
					player.ItemCheck(player.whoAmI);
				}
				else
					Main.PlaySound(39, (int)player.Center.X, (int)player.Center.Y, Main.rand.Next(3));
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
			mouseRightHeld = Main.mouseRight;

			if (mouseRightPressed) {
				//Main.NewText("" + Main.time + ": just pressed right mouse");
				lastRightClicks.Add(100);
			}

			justOpenedInventory = PlayerInput.Triggers.JustPressed.Inventory && !Main.playerInventory;
		}

		public override void PreUpdate() {
			if (player.whoAmI == Main.myPlayer) {
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
			for (int i = 0; i < lastRightClicks.Count; i++) {
				lastRightClicks[i] -= 2;
			}
			lastRightClicks.RemoveAll(x => x <= 0);
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

		// Not working in MP for some reason. Too early?
		public override void OnEnterWorld(Player player) => UseAutomaticHistoryBook();
	}
}
