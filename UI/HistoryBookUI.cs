using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SummonersAssociation.Models;
using SummonersAssociation.Items;
using Terraria.ID;

namespace SummonersAssociation.UI
{
	/*
	* How it works basically:
	* In SAPlayer.ProcessTriggers(), it sets two bools used to open/close the UI.
	* SAPlayer.PreUpdate() handles the opening/closing, and general handling of the UI.
	* The other custom methods are all over the place right now (Some in this class, some in the SAPlayer class)
	* Caveats:
	* 1. spawning the UI in ProcessTriggers didn't work because the custom conditions I setup
	* don't work there (AllowedToOpenHistoryBookUI)
	* 2. Read comments in ProcessTriggers
	*/
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
		/// Which thing is currently highlighted?
		/// </summary>
		internal static int returned = NONE;

		/// <summary>
		/// Is cursor currently in the middle or not?
		/// </summary>
		internal static bool middle = true;

		/// <summary>
		/// Was the delete initiated once?
		/// </summary>
		internal static bool aboutToDelete = false;

		/// <summary>
		/// Fade in animation when opening the UI
		/// </summary>
		internal static float fadeIn = 0;

		/// <summary>
		/// Red cross for when to reset, unused
		/// </summary>
		internal static Texture2D redCrossTexture;

		/// <summary>
		/// Holds data about what to draw
		/// </summary>
		internal static List<ItemModel> itemModels;

		/// <summary>
		/// Is cursor within a segment?
		/// </summary>
		public static bool IsMouseWithinAnySegment => !middle && returned > NONE;

		/// <summary>
		/// Spawn position offset to top left corner of that to draw the icons
		/// </summary>
		private static Vector2 TopLeftCorner => spawnPosition - new Vector2(mainRadius, mainRadius);

		//Update, unused
		public override void Update(GameTime gameTime) => base.Update(gameTime);

		//Draw
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Main.LocalPlayer.mouseInterface = true;

			int outerRadius = 48;
			if (itemModels.Count > 5) outerRadius += 6 * (itemModels.Count - 5); //increase by 6 after having more than 5 options, starts getting clumped at about 30(?) circles
			if (fadeIn < outerRadius) outerRadius = (int)(fadeIn += (float)outerRadius / 10);

			int width;
			int height;
			double angleSteps = 2.0d / itemModels.Count;
			double x;
			double y;
			ItemModel itemModel;

			//done --> Index of currently drawn circle
			//Starts at the top and goes clockwise
			for (int done = 0; done < itemModels.Count; done++) {
				itemModel = itemModels[done];
				x = outerRadius * Math.Sin(angleSteps * done * Math.PI);
				y = outerRadius * -Math.Cos(angleSteps * done * Math.PI);

				var bgRect = new Rectangle((int)(TopLeftCorner.X + x), (int)(TopLeftCorner.Y + y), mainDiameter, mainDiameter);
				//Check if mouse is within the circle checked
				bool isMouseWithinSegment = CheckMouseWithinWheelSegment(Main.MouseScreen, spawnPosition, mainRadius, itemModels.Count, done);

				//Actually draw the bg circle
				Color drawColor = Color.White;
				if (!itemModel.Active) drawColor = Color.Gray;
				spriteBatch.Draw(Main.wireUITexture[isMouseWithinSegment ? 1 : 0], bgRect, drawColor);

				//Draw sprites over the icons
				Texture2D itemTexture = Main.itemTexture[itemModel.ItemType];
				width = itemTexture.Width;
				height = itemTexture.Height;
				var projRect = new Rectangle((int)(spawnPosition.X + x) - (width / 2), (int)(spawnPosition.Y + y) - (height / 2), width, height);

				spriteBatch.Draw(itemTexture, projRect, itemTexture.Bounds, drawColor);

				if (isMouseWithinSegment) {
					//Set the "returned" new type
					returned = done;
				}
			}

			//Draw held item bg circle
			var outputRect = new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, mainDiameter, mainDiameter);

			middle = CheckMouseWithinCircle(Main.MouseScreen, spawnPosition, mainRadius);

			spriteBatch.Draw(Main.wireUITexture[middle ? 1 : 0], outputRect, Color.White);

			//Draw held item inside circle
			Texture2D historyBookTexture = Main.itemTexture[SummonersAssociation.Instance.ItemType<MinionHistoryBook>()];
			width = historyBookTexture.Width;
			height = historyBookTexture.Height;
			var outputWeaponRect = new Rectangle((int)spawnPosition.X - (width / 2), (int)spawnPosition.Y - (height / 2), width, height);
			spriteBatch.Draw(historyBookTexture, outputWeaponRect, Color.White);

			//Draw the number (summonCountTotal)
			Color fontColor = Color.White;
			Vector2 mousePos = new Vector2(16, 16) + Main.MouseScreen;

			Vector2 drawPos = new Vector2((int)TopLeftCorner.X, (int)TopLeftCorner.Y + height) + new Vector2(-4, - 8);
			summonCountDelta = summonCountTotal - SumSummonCounts();

			if (summonCountDelta < 0) fontColor = Color.Red;
			string tooltip = summonCountDelta.ToString() + "/" + summonCountTotal.ToString();

			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);

			fontColor = Color.White;

			//Extra loop so tooltips are always drawn after the circles
			for (int done = 0; done < itemModels.Count; done++) {
				itemModel = itemModels[done];
				bool isMouseWithinSegment = CheckMouseWithinWheelSegment(Main.MouseScreen, spawnPosition, mainRadius, itemModels.Count, done);

				if (isMouseWithinSegment) {
					//Draw the tooltip
					drawPos = mousePos;
					tooltip = itemModel.Name;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);
				}

				//Draw the number (SummonCount)
				x = outerRadius * Math.Sin(angleSteps * done * Math.PI);
				y = outerRadius * -Math.Cos(angleSteps * done * Math.PI);

				drawPos = new Vector2((int)(TopLeftCorner.X + x), (int)(TopLeftCorner.Y + y)) + new Vector2(-4, mainRadius + 4);
				tooltip = itemModel.SummonCount.ToString();

				if (!itemModel.Active) fontColor = Color.Red;

				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);
			}

			//If hovering over the middle
			if (middle) {
				returned = NONE;
				fontColor = Color.White;
				drawPos = mousePos;

				if (aboutToDelete) {
					//Draw the red cross
					width = redCrossTexture.Width;
					height = redCrossTexture.Height;
					var crossRect = new Rectangle((int)spawnPosition.X - (width / 2), (int)spawnPosition.Y - (height / 2), width, height);
					spriteBatch.Draw(redCrossTexture, crossRect, Color.White);

					//Draw the tooltip
					tooltip = "Click left to clear history";
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);

					drawPos.Y += 22;
					tooltip = "Click right to cancel";
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);
				}
				else {
					returned = UPDATE;

					//Draw the tooltip
					drawPos = mousePos;
					tooltip = "Click left twice to clear history";
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);
					drawPos.Y += 22;
					tooltip = "Click right to save history";
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, drawPos, fontColor, 0, Vector2.Zero, Vector2.One);
				}
			}
		}

		/// <summary>
		/// Check if the mouse cursor is within the radius around the position specified by center
		/// </summary>
		internal static bool CheckMouseWithinCircle(Vector2 mousePos, Vector2 center, int radius)
			=> ((mousePos.X - center.X) * (mousePos.X - center.X) + (mousePos.Y - center.Y) * (mousePos.Y - center.Y)) <= radius * radius;

		/// <summary>
		/// Checks if the mouse cursor is currently inside the segment specified by the arguments. Decided by angle (radius only matters for the inner element).
		/// </summary>
		internal static bool CheckMouseWithinWheelSegment(Vector2 mousePos, Vector2 center, int innerRadius, int pieceCount, int elementNumber) {
			//Check if mouse cursor is outside the inner circle
			bool outsideInner = !CheckMouseWithinCircle(mousePos, center, innerRadius);

			if (!outsideInner) return false;

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
		/// Creates a list of summon weapons from the players inventory, ignoring duplicates
		/// </summary>
		public static List<ItemModel> GetSummonWeapons() {
			var list = new List<ItemModel>();
			for (int i = 0; i < Main.maxInventory; i++) {
				Item item = Main.LocalPlayer.inventory[i];
				if (item.summon && list.FindIndex(itemModel => itemModel.ItemType == item.type) < 0) {
					//ItemModels added here have SummonCount set to 0, will be checked later in Start() and adjusted
					list.Add(new ItemModel(item, i));
				}
			}
			return list;
		}

		public static List<ItemModel> MergeHistoryIntoInventory(MinionHistoryBook book) {
			List<ItemModel> historyCopy = book.history.ConvertAll(model => new ItemModel(model));
			return MergeHistoryIntoInventory(historyCopy);
		}

		/// <summary>
		/// Returns an "updated" history and removes duplicate entries from the passed history
		/// </summary>
		public static List<ItemModel> MergeHistoryIntoInventory(List<ItemModel> history) {
			//Get all summon weapons currently in inventory
			List<ItemModel> inventoryModels = GetSummonWeapons();

			//If (inventoryModels.Count <= 0 && history.Count <= 0) return ;

			//From now on, inventoryModels is going to be the list passed into the UI
			List<ItemModel> passedModels = inventoryModels;

			//Adjust the list passed to the UI in a way that matches the history of items that were
			//once used but aren't in the inventory anymore,
			//and those just found
			for (int i = 0; i < passedModels.Count; i++) {
				ItemModel itemModel = passedModels[i];
				int index = history.FindIndex(model => model.ItemType == itemModel.ItemType);
				if (index > -1) {
					itemModel.OverrideValuesFromHistory(history[index]);
					history.RemoveAt(index);
				}
			}

			//Here, history only contains "old" items that don't exist in the inventory
			//set their InventoryIndex to a high value (so they are all sorted last)
			//and add them in

			for (int i = 0; i < history.Count; i++) {
				history[i].OverrideValuesToInactive(i);
			}

			passedModels.AddRange(history);
			//Sorted by InventoryIndex
			passedModels.Sort();

			return passedModels;
		}

		/// <summary>
		/// Called when the UI is about to appear
		/// </summary>
		public static bool Start() {
			List<ItemModel> historyCopy = ((MinionHistoryBook)Main.LocalPlayer.HeldItem.modItem).history.ConvertAll(model => new ItemModel(model));
			List<ItemModel> passedModels = MergeHistoryIntoInventory(historyCopy);

			visible = true;
			spawnPosition = SummonersAssociation.MousePositionUI;
			heldItemIndex = Main.LocalPlayer.selectedItem;
			itemModels = passedModels;

			try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
			catch { /*No idea why but this threw errors one time*/ }

			return true;
		}
		
		/// <summary>
		 /// Called to close the UI
		 /// </summary>
		public static void Stop(bool playSound = true) {
			returned = NONE;
			fadeIn = 0;
			aboutToDelete = false;
			visible = false;

			if (playSound) {
				try { Main.PlaySound(SoundID.Item1, Main.LocalPlayer.position); }
				catch { /*No idea why but this threw errors one time*/ }
			}
		}

		/// <summary>
		/// Sets the history of the passed History Book based on the one in the UI
		/// </summary>
		public static void UpdateHistoryBook(MinionHistoryBook book)
			=> book.history = itemModels.ConvertAll((itemModel) => new ItemModel(itemModel));

		public static int SumSummonCounts() {
			int sum = 0;
			for (int i = 0; i < itemModels.Count; i++) {
				ItemModel itemModel = itemModels[i];
				if (itemModel.Active) {
					sum += itemModel.SummonCount;
				}
			}
			return sum;
		}
	}
}
