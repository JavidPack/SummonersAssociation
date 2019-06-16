using Terraria.UI;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SummonersAssociation.Models;
using SummonersAssociation.Items;

namespace SummonersAssociation.UI
{
	/*
	* How it works basically:
	* In SAPlayer.ProcessTriggers(), it sets two bools used to open/close the UI.
	* SAPlayer.PostUpdate() handles the opening/closing, and general handling of the UI.
	* The other custom methods are all over the place right now (Some in this class, some in the SAPlayer class)
	* Caveats:
	* 1. spawning the UI in ProcessTriggers didn't work because the custom conditions I setup
	* don't work there (AllowedToOpenHistoryBookUI)
	* 2. can't think of more right now, but there are some others
	*/
	class HistoryBookUI : UIState
	{
		//output
		internal const int NONE = -1;

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
		/// Held item index
		/// </summary>
		internal static int heldItemIndex = -1;

		/// <summary>
		/// Which thing is currently highlighted?
		/// </summary>
		internal static int returned = NONE;

		/// <summary>
		/// Which thing was the previously selected one?
		/// </summary>
		internal static int lastSelected = NONE;

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
		/// Spawn position offset to top left corner of that to draw the icons
		/// </summary>
		private Vector2 TopLeftCorner => spawnPosition - new Vector2(mainRadius, mainRadius);

		//Update, unused
		public override void Update(GameTime gameTime) => base.Update(gameTime);

		//Draw
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Main.LocalPlayer.mouseInterface = true;

			int outerRadius = 48;
			if (itemModels.Count > 5) outerRadius += 5 * (itemModels.Count - 5); //increase by 5 after having more than 5 options, starts getting clumped at about 24 circles
			if (fadeIn < outerRadius) outerRadius = (int)(fadeIn += (float)outerRadius / 10);

			double angleSteps = 2.0d / itemModels.Count;
			//done --> ID of currently drawn circle
			for (int done = 0; done < itemModels.Count; done++) {
				double x = outerRadius * Math.Sin(angleSteps * done * Math.PI);
				double y = outerRadius * -Math.Cos(angleSteps * done * Math.PI);

				var bgRect = new Rectangle((int)(TopLeftCorner.X + x), (int)(TopLeftCorner.Y + y), mainDiameter, mainDiameter);
				//Check if mouse is within the circle checked
				bool isMouseWithin = CheckMouseWithinWheel(Main.MouseScreen, spawnPosition, mainRadius, outerRadius, itemModels.Count, done);

				//Actually draw the bg circle
				Color drawColor = Color.White;
				if (done == lastSelected) drawColor = Color.Gray;
				spriteBatch.Draw(Main.wireUITexture[isMouseWithin ? 1 : 0], bgRect, drawColor);

				//Draw sprites over the icons
				Texture2D itemTexture = Main.itemTexture[itemModels[done].ItemType];
				int width = itemTexture.Width;
				int height = itemTexture.Height;
				var projRect = new Rectangle((int)(spawnPosition.X + x) - (width / 2), (int)(spawnPosition.Y + y) - (height / 2), width, height);

				//drawColor = Color.White;
				//if (done == lastSelected) drawColor = Color.Gray;

				spriteBatch.Draw(itemTexture, projRect, itemTexture.Bounds, drawColor);

				if (isMouseWithin) {
					//Set the "returned" new type
					returned = done;
				}
			}

			//Draw held item bg circle
			var outputRect = new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, mainDiameter, mainDiameter);

			bool middle = CheckMouseWithinCircle(Main.MouseScreen, mainRadius, spawnPosition);

			spriteBatch.Draw(Main.wireUITexture[middle ? 1 : 0], outputRect, Color.White);

			//Draw held item inside circle
			Texture2D historyBookTexture = Main.itemTexture[SummonersAssociation.Instance.ItemType<MinionHistoryBook>()];
			int finalWidth = historyBookTexture.Width;
			int finalHeight = historyBookTexture.Height;
			var outputWeaponRect = new Rectangle((int)spawnPosition.X - (finalWidth / 2), (int)spawnPosition.Y - (finalHeight / 2), finalWidth, finalHeight);
			spriteBatch.Draw(historyBookTexture, outputWeaponRect, Color.White);

			if (middle) {
				//If hovering over the middle, reset maybe
				//Currently, don't return anything
				returned = NONE;

				//if (hasEquipped) {
				//	//Draw the red cross
				//	int finalWidth = redCrossTexture.Width;
				//	int finalHeight = redCrossTexture.Height;
				//	Rectangle outputCrossRect = new Rectangle((int)spawnPosition.X - (finalWidth / 2), (int)spawnPosition.Y - (finalHeight / 2), finalWidth, finalHeight);
				//	spriteBatch.Draw(redCrossTexture, outputCrossRect, Color.White);

				//	//Draw the tooltip
				//	Color fontColor = Color.White;
				//	Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
				//	ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, "Unequip", mousePos + new Vector2(16, 16), fontColor, 0, Vector2.Zero, Vector2.One);
				//}
			}

			//Extra loop so tooltips are always drawn after the circles
			for (int done = 0; done < itemModels.Count; done++) {
				bool isMouseWithin = CheckMouseWithinWheel(Main.MouseScreen, spawnPosition, mainRadius, outerRadius, itemModels.Count, done);
				string tooltip = itemModels[done].Name;

				if (isMouseWithin) {
					//Draw the tooltip
					Color fontColor = Color.White;
					Vector2 mousePos = new Vector2(16, 16) + Main.MouseScreen;
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, tooltip, mousePos, fontColor, 0, Vector2.Zero, Vector2.One);
				}
			}
		}

		/// <summary>
		/// Check if the mouse cursor is within the radius around the position specified by center
		/// </summary>
		internal static bool CheckMouseWithinCircle(Vector2 mousePos, int radius, Vector2 center)
			=> ((mousePos.X - center.X) * (mousePos.X - center.X) + (mousePos.Y - center.Y) * (mousePos.Y - center.Y)) <= radius * radius;

		/// <summary>
		/// Checks if the mouse cursor is currently inside the segment specified by the arguments. Decided by angle (radius only matters for the inner element).
		/// </summary>
		internal static bool CheckMouseWithinWheel(Vector2 mousePos, Vector2 center, int innerRadius, int outerRadius, int pieceCount, int elementNumber) {
			//Check if mouse cursor is outside the inner circle
			bool outsideInner = ((mousePos.X - center.X) * (mousePos.X - center.X) + (mousePos.Y - center.Y) * (mousePos.Y - center.Y)) > innerRadius * innerRadius;

			//padding
			outerRadius += mainRadius;
			//Check if mouse cursor is inside the outer circle
			bool insideOuter = ((mousePos.X - center.X) * (mousePos.X - center.X) + (mousePos.Y - center.Y) * (mousePos.Y - center.Y)) < outerRadius * outerRadius;
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
		/// Creates a list of summon weapons from the players inventory, ignoring duplicates
		/// </summary>
		private static bool GetSummonWeapons() {
			var list = new List<ItemModel>();
			for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++) {
				Item item = Main.LocalPlayer.inventory[i];
				if (item.summon && list.FindIndex(itemModel => itemModel.ItemType == item.type) < 0) {
					list.Add(new ItemModel(item, i));
				}
			}
			itemModels = list;
			return itemModels.Count > 0;
		}

		/// <summary>
		/// Called when the UI is about to appear
		/// </summary>
		public static bool Start() {
			//Get all summon weapons
			if (!GetSummonWeapons()) return false;

			visible = true;
			spawnPosition = SummonersAssociation.MousePositionUI;
			heldItemIndex = Main.LocalPlayer.selectedItem;
			//We know the HeldItem is the book, so directly cast it
			int currentSel = ((MinionHistoryBook)Main.LocalPlayer.HeldItem.modItem).itemModel.ItemType;
			lastSelected = itemModels.FindIndex(item => item.ItemType == currentSel);
			return true;
		}
		
		/// <summary>
		 /// Called to forcibly close the UI
		 /// </summary>
		public static void Stop() {
			returned = NONE;
			fadeIn = 0;
			visible = false;
		}

		/// <summary>
		/// Sets currentMinionWeaponType in all History Book items to the returned type
		/// </summary>
		public static void UpdateHistoryBookItemsInInventory() {
			if (returned > NONE) {
				for (int i = 0; i < Main.LocalPlayer.inventory.Length; i++) {
					Item item = Main.LocalPlayer.inventory[i];
					if (item.type == SummonersAssociation.Instance.ItemType<MinionHistoryBook>()) {
						var mItem = (MinionHistoryBook)item.modItem;
						mItem.itemModel = itemModels[returned].Clone();
						//mItem.currentMinionWeaponType = itemModels[returned].ItemType;
						//mItem.testStringName = itemModels[returned].Name;
						Main.NewText("set model in item at " + i);
						Main.NewText(mItem.itemModel);
					}
				}
			}
		}
	}
}
