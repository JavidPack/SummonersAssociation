using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummonersAssociation.Items;
using SummonersAssociation.Models;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace SummonersAssociation.UI
{
	/*
	* How it works basically:
	* In SAPlayer.ProcessTriggers, it sets two bools used to open/close the UI.
	* SummonersAssociation.PreUpdate handles the opening/closing, and general handling of the UI.
	* The other custom methods are all over the place right now (Some in this class, some in the SummonersAssociation class)
	* Caveats:
	* 1. spawning the UI in ProcessTriggers didn't work because the custom conditions I setup
	* don't work there (AllowedToOpenHistoryBookUI)
	* 2. Read comments in ProcessTriggers
	*/

	/// <summary>
	/// UIState for the history book that handles all its logic in DrawSelf
	/// </summary>
	class HistoryBookUI : UIState
	{
		//output
		internal const int NONE = -1;

		internal const int UPDATE = -2;

		/// <summary>
		/// Circle diameter
		/// </summary>
		internal const int mainDiameter = 36;

		/// <summary>
		/// Circle radius
		/// </summary>
		internal const int mainRadius = mainDiameter / 2;

		/// <summary>
		/// Is the UI visible?
		/// </summary>
		internal static bool visible = false;

		/// <summary>
		/// Spawn position, i.e. mouse position at UI start
		/// </summary>
		internal static Vector2 spawnPosition = default(Vector2);

		/// <summary>
		/// Number of casts in total (player.numMinions)
		/// </summary>
		internal static int summonCountTotal = -1;

		/// <summary>
		/// Difference of summonCountTotal - SumSummonCounts()
		/// </summary>
		internal static int summonCountDelta = 0;

		/// <summary>
		/// Held item index
		/// </summary>
		internal static int heldItemIndex = -1;

		/// <summary>
		/// Held item type
		/// </summary>
		internal static int heldItemType = -1;

		/// <summary>
		/// Which thing is currently highlighted?
		/// </summary>
		internal static int returned = NONE;

		/// <summary>
		/// Which thing is currently selected? (Simple only)
		/// </summary>
		internal static int selected = NONE;

		/// <summary>
		/// Is this the simple UI without numbers and extra tooltips?
		/// </summary>
		internal static bool simple = false;

		/// <summary>
		/// Is cursor currently in the middle or not?
		/// </summary>
		internal static bool middle = true;

		/// <summary>
		/// Is cursor currently inside the outer radius of the UI?
		/// </summary>
		internal static bool isMouseWithinUI = false;

		/// <summary>
		/// Was the delete initiated once?
		/// </summary>
		internal static bool aboutToDelete = false;

		/// <summary>
		/// Fade in animation when opening the UI
		/// </summary>
		internal static float fadeIn = 0f;

		/// <summary>
		/// Fade in for the SummonCountTotal when trying to increase a SummonCount beyond limit
		/// </summary>
		internal static float colorFadeIn = 0f;

		/// <summary>
		/// Red cross for when to reset
		/// </summary>
		internal static Texture2D redCrossTexture;

		/// <summary>
		/// Holds data about each ItemModel
		/// </summary>
		internal static List<ItemModel> itemModels;

		/// <summary>
		/// Holds data about what to draw (the outer ring)
		/// </summary>
		internal static List<UIModel> uiModels;

		/// <summary>
		/// Is cursor within a segment?
		/// </summary>
		internal static bool IsMouseWithinAnySegment => isMouseWithinUI && !middle && returned > NONE;

		/// <summary>
		/// Spawn position offset to top left corner of that to draw the icons
		/// </summary>
		internal static Vector2 TopLeftCorner => spawnPosition - new Vector2(mainRadius, mainRadius);

		public override void OnInitialize() => uiModels = new List<UIModel>();

		public override void Update(GameTime gameTime) {
			if (!visible) return;
			base.Update(gameTime);
			Main.LocalPlayer.mouseInterface = true;

			int outerRadius = 48;
			//Increase by 6 after having more than 5 options, starts getting clumped at about 30(?) circles
			if (itemModels.Count > 5) outerRadius += 6 * (itemModels.Count - 5);
			if (fadeIn < outerRadius) outerRadius = (int)(fadeIn += (float)outerRadius / 10);
			//TODO Fix "snapping back" when close to outerRadius at high itemModels.Count

			if (colorFadeIn > 0f) colorFadeIn -= 0.036f;

			//Set some values that will be accessed here and outside the UI
			middle = CheckMouseWithinCircle(Main.MouseScreen, spawnPosition, mainRadius);

			isMouseWithinUI = CheckMouseWithinCircle(Main.MouseScreen, spawnPosition, outerRadius + mainRadius);

			summonCountDelta = GetSummonCountDelta();

			int width;
			int height;
			double angleSteps = 2.0d / itemModels.Count;
			double x;
			double y;
			bool isMouseWithinSegment;
			List<string> tooltip;
			string number = "";
			Rectangle bgRect;
			Rectangle itemRect;
			Color bgColor;
			Color itemColor;
			Color numberColor = default;
			ItemModel itemModel;
			Texture2D texture;

			uiModels.Clear();

			//done --> Index of currently drawn circle
			//Starts at the top and goes clockwise
			for (int done = 0; done < itemModels.Count; done++) {
				x = outerRadius * Math.Sin(angleSteps * done * Math.PI);
				y = outerRadius * -Math.Cos(angleSteps * done * Math.PI);
				itemModel = itemModels[done];
				tooltip = new List<string>();

				isMouseWithinSegment = CheckMouseWithinWheelSegment(Main.MouseScreen, spawnPosition, mainRadius, outerRadius, itemModels.Count, done);

				if (isMouseWithinSegment) {
					//Set the returned thing
					returned = done;

					#region Setup weapon tooltip
					tooltip.Add(itemModel.Name);

					if (itemModel.SlotsNeeded > 1) {
						tooltip.Add("Slots required: " + itemModel.SlotsNeeded);
					}

					if (simple && selected == done) {
						tooltip.Add("(Selected)");

						if (!itemModel.Active) {
							tooltip.Add("Not found in inventory");
						}
					}
					#endregion
				}

				#region Setup weapon background circle
				bgColor = Color.White;
				if (!itemModel.Active) bgColor = Color.Gray;
				if (selected == done) {
					if (itemModel.Active) bgColor = Color.LimeGreen;
					else bgColor = Color.Red;
				}
				bgRect = new Rectangle((int)(TopLeftCorner.X + x), (int)(TopLeftCorner.Y + y), mainDiameter, mainDiameter);
				#endregion

				#region Setup weapon sprite
				texture = Main.itemTexture[itemModel.ItemType];
				width = texture.Width;
				height = texture.Height;
				itemColor = Color.White;
				if (selected == done) {
					if (itemModel.Active) itemColor = Color.White;
					else itemColor = Color.Gray;
				}
				itemRect = new Rectangle((int)(spawnPosition.X + x) - (width / 2), (int)(spawnPosition.Y + y) - (height / 2), width, height);
				#endregion

				#region Setup SummonCount
				if (!simple) {
					if (isMouseWithinSegment && colorFadeIn > 0f) {
						//Do a fade out on the number of this segment if clicked when it can't be incremented
						numberColor = new Color(Color.White.ToVector4() * (1f - colorFadeIn) + Color.Red.ToVector4() * colorFadeIn);
					}
					else {
						numberColor = Color.White;
					}
					number = itemModel.SummonCount.ToString();

					if (!itemModel.Active) numberColor = Color.Red;
				}
				#endregion

				uiModels.Add(new UIModel(
					isMouseWithinSegment,
					itemModel.ItemType,
					bgRect,
					bgColor,
					itemRect,
					itemColor,
					tooltip,
					numberColor,
					number));
			}

			//If hovering over the middle
			if (middle) {
				returned = NONE;
				if (!aboutToDelete) {
					returned = UPDATE;
				}
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (!visible) return;
			base.DrawSelf(spriteBatch);

			int width;
			int height;
			string middleTip;
			Texture2D texture;
			Rectangle outputRect;
			Color fontColor;
			Vector2 mousePos = new Vector2(16) + Main.MouseScreen;
			Vector2 drawPos;

			#region Draw each UIModel
			foreach (UIModel model in uiModels) {
				//Draw weapon background circle
				spriteBatch.Draw(Main.wireUITexture[model.Mouseover ? 1 : 0], model.BackgroundRect, model.BackgroundColor);

				//Draw weapon sprite
				texture = Main.itemTexture[model.ItemType];
				spriteBatch.Draw(texture, model.ItemRect, texture.Bounds, model.ItemColor);

				//Draw SummonCount
				if (!simple) {
					drawPos = model.BackgroundRect.BottomLeft() + new Vector2(-4, -mainRadius + 4);
					DrawText(spriteBatch, model.Number, drawPos, model.NumberColor);
				}
			}
			#endregion

			#region Draw book background circle
			outputRect = new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, mainDiameter, mainDiameter);
			spriteBatch.Draw(Main.wireUITexture[middle ? 1 : 0], outputRect, Color.White);
			#endregion

			#region Draw book sprite
			texture = Main.itemTexture[heldItemType];
			width = texture.Width;
			height = texture.Height;
			outputRect = new Rectangle((int)spawnPosition.X - (width / 2), (int)spawnPosition.Y - (height / 2), width, height);
			spriteBatch.Draw(texture, outputRect, Color.White);
			#endregion

			#region Draw summonCountTotal
			if (!simple) {
				if (colorFadeIn > 0f) {
					//Do a fade out on the number if clicked when it can't be incremented
					fontColor = new Color(Color.White.ToVector4() * (1f - colorFadeIn) + Color.Red.ToVector4() * colorFadeIn);
				}
				else {
					fontColor = Color.White;
				}
				drawPos = new Vector2((int)TopLeftCorner.X, (int)TopLeftCorner.Y + height) + new Vector2(-4, mainRadius - 20);

				if (summonCountDelta < 0) fontColor = Color.Red;
				middleTip = summonCountDelta.ToString() + "/" + summonCountTotal.ToString();

				DrawText(spriteBatch, middleTip, drawPos, fontColor);
			}
			#endregion

			#region Draw weapon tooltips
			foreach (UIModel model in uiModels) {
				drawPos = mousePos;
				foreach (string tooltip in model.Tooltip) {
					DrawText(spriteBatch, tooltip, drawPos, Color.White);
					drawPos.Y += 24;
				}
			}
			#endregion

			#region Draw middle tooltip, red cross
			if (middle) {
				fontColor = Color.White;
				drawPos = mousePos;

				if (aboutToDelete) {
					#region Draw red cross
					width = redCrossTexture.Width;
					height = redCrossTexture.Height;
					outputRect = new Rectangle((int)spawnPosition.X - (width / 2), (int)spawnPosition.Y - (height / 2), width, height);
					spriteBatch.Draw(redCrossTexture, outputRect, Color.White);
					#endregion

					#region Draw tooltip for what happens on click
					if (!simple) {
						middleTip = "Click left to clear history";
						DrawText(spriteBatch, middleTip, drawPos, fontColor);

						drawPos.Y += 24;
						middleTip = "Click right to cancel";
						DrawText(spriteBatch, middleTip, drawPos, fontColor);
					}
					#endregion
				}
				else {
					#region Draw tooltip for what happens on click
					if (!simple) {
						drawPos = mousePos;
						middleTip = "Click left twice to clear history";
						DrawText(spriteBatch, middleTip, drawPos, fontColor);
						drawPos.Y += 24;
						middleTip = "Click right to save history";
						DrawText(spriteBatch, middleTip, drawPos, fontColor);
					}
					#endregion
				}
			}
			#endregion
		}

		/// <summary>
		/// Check if the mouse cursor is within the radius around the position specified by center
		/// </summary>
		internal static bool CheckMouseWithinCircle(Vector2 mousePos, Vector2 center, int radius)
			=> Vector2.DistanceSquared(mousePos, center) <= radius * radius;

		/// <summary>
		/// Checks if the mouse cursor is currently inside the segment specified by the arguments. Decided by angle (radius only matters for the inner element).
		/// </summary>
		internal static bool CheckMouseWithinWheelSegment(Vector2 mousePos, Vector2 center, int innerRadius, int outerRadius, int pieceCount, int elementNumber) {
			//Check if mouse cursor is outside the inner circle
			bool outsideInner = !CheckMouseWithinCircle(mousePos, center, innerRadius);

			//Padding
			outerRadius += mainRadius;
			//Check if mouse cursor is inside the outer circle
			bool insideOuter = CheckMouseWithinCircle(mousePos, center, outerRadius);
			if (!outsideInner || !insideOuter) return false;

			double step = 360 / pieceCount;
			double finalOffset = -step / 2;

			double beginAngle = (finalOffset + step * elementNumber) % 360;
			double endAngle = (beginAngle + step) % 360;
			if (beginAngle < 0) beginAngle = 360 + beginAngle;

			//Calculate x,y coords on outer circle
			double calculatedAngle = Math.Atan2(mousePos.X - center.X, -(mousePos.Y - center.Y));
			calculatedAngle = calculatedAngle * 180 / Math.PI;

			if (calculatedAngle < 0) {
				calculatedAngle = 360 + calculatedAngle;
			}

			bool insideSegment = false;
			if (beginAngle < endAngle) {
				if (calculatedAngle > beginAngle && calculatedAngle < endAngle) {
					insideSegment = true;
				}
			}
			else {
				if (calculatedAngle > beginAngle && calculatedAngle > endAngle) {
					insideSegment = true;
				}
				if (calculatedAngle < beginAngle && calculatedAngle < endAngle) {
					insideSegment = true;
				}
			}

			return insideSegment;
		}

		/// <summary>
		/// Shorter version of DrawColorCodedStringWithShadow()
		/// </summary>
		internal static void DrawText(SpriteBatch sb, string tt, Vector2 pos, Color color)
			=> ChatManager.DrawColorCodedStringWithShadow(sb, Main.fontMouseText, tt, pos, color, 0, Vector2.Zero, Vector2.One);

		/// <summary>
		/// Creates a list of summon weapons from the players inventory, ignoring duplicates
		/// </summary>
		public static List<ItemModel> GetSummonWeapons() {
			var list = new List<ItemModel>();
			for (int i = 0; i < Main.maxInventory; i++) {
				Item item = Main.LocalPlayer.inventory[i];
				if (item.type != ItemID.Count && item.summon && !item.sentry && list.FindIndex(itemModel => itemModel.ItemType == item.type) < 0) {
					//Exclude sentry weapons, maybe separate item later
					//ItemModels added here have SummonCount set to 0, will be checked later in Start() and adjusted
					list.Add(new ItemModel(item, i));
				}
			}
			return list;
		}

		/// <summary>
		/// Returns an "updated" history and removes duplicate entries from the passed history
		/// </summary>
		public static List<ItemModel> MergeHistoryIntoInventory(MinionHistoryBookSimple book) {
			List<ItemModel> historyCopy = book.history.ConvertAll(model => new ItemModel(model));
			return MergeHistoryIntoInventory(historyCopy);
		}

		/// <summary>
		/// Returns an "updated" history and removes duplicate entries from the passed history
		/// </summary>
		public static List<ItemModel> MergeHistoryIntoInventory(List<ItemModel> history) {
			//Get all summon weapons currently in inventory
			List<ItemModel> passedModels = GetSummonWeapons();

			ItemModel itemModel;

			//Adjust the list passed to the UI in a way that matches the history of items that were
			//once used but aren't in the inventory anymore,
			//and those just found
			for (int i = 0; i < passedModels.Count; i++) {
				itemModel = passedModels[i];
				int index = history.FindIndex(model => model.ItemType == itemModel.ItemType);
				if (index > -1) {
					itemModel.OverrideValuesFromHistory(history[index]);
					history.RemoveAt(index);
				}
			}

			//Here, history only contains "old" items that don't exist in the inventory
			//set their InventoryIndex to a high value (so they are all sorted last)
			//and add them in if there was atleast one summonCount specified,
			//or this is the simple book

			for (int i = 0; i < history.Count; i++) {
				itemModel = history[i];
				itemModel.OverrideValuesToInactive(i);
				//If simple, keep "last selected" item in the UI
				if (itemModel.SummonCount > 0 || simple) passedModels.Add(itemModel);
			}

			//Sorted by InventoryIndex
			passedModels.Sort();

			return passedModels;
		}

		/// <summary>
		/// Sets the history of the passed History Book based on the one in the UI
		/// </summary>
		public static void UpdateHistoryBook(MinionHistoryBookSimple book) {
			if (simple && selected != NONE) {
				//Only add the first element as the selected one
				book.history = new List<ItemModel> {
					new ItemModel(itemModels[selected])
				};
				book.history[0].SummonCount = 1;
			}
			else {
				book.history = itemModels.ConvertAll((itemModel) => new ItemModel(itemModel));
			}
		}

		/// <summary>
		/// summonCountTotal minus all the summon counts weighted with the slots needed
		/// </summary>
		public static int GetSummonCountDelta() {
			int sum = summonCountTotal;
			for (int i = 0; i < itemModels.Count; i++) {
				ItemModel itemModel = itemModels[i];
				if (itemModel.Active) {
					sum -= itemModel.SummonCount * itemModel.SlotsNeeded;
				}
			}
			return sum;
		}

		/// <summary>
		/// Called when the UI is about to appear
		/// </summary>
		public static bool Start() {
			List<ItemModel> history = ((MinionHistoryBookSimple)Main.LocalPlayer.HeldItem.modItem).history;
			List<ItemModel> historyCopy = history.ConvertAll(model => new ItemModel(model));
			List<ItemModel> passedModels = MergeHistoryIntoInventory(historyCopy);

			itemModels = passedModels;

			if (itemModels.Count < 1) return false;

			visible = true;
			spawnPosition = SummonersAssociation.MousePositionUI;
			heldItemIndex = Main.LocalPlayer.selectedItem;
			heldItemType = Main.LocalPlayer.HeldItem.type;

			simple = Array.IndexOf(SummonersAssociation.BookTypes, heldItemType) == 0;
			uiModels.Clear();

			if (simple && history.Count > 0) {
				//Set the selected index to what the original history has
				//by definition, if not found it is -1 == NONE
				selected = itemModels.FindIndex(model => model.ItemType == history[0].ItemType);
			}

			try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
			catch { /*No idea why but this threw errors one time*/ }

			return true;
		}

		/// <summary>
		/// Called to close the UI
		/// </summary>
		public static void Stop(bool playSound = true) {
			returned = NONE;
			selected = NONE;
			fadeIn = 0f;
			colorFadeIn = 0f;
			aboutToDelete = false;
			visible = false;

			if (playSound) {
				try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
				catch { /*No idea why but this threw errors one time*/ }
			}
		}
	}
}
