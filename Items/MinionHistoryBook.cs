using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBook : MinionHistoryBookSimple
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion History Book");
			//TODO
			Tooltip.SetDefault("TODO"
				+ "\nLeft click to summon minions based on history"
				+ "\nRight click to open an UI"
				+ "\nLeft/Right click on the item icons to adjust the summon count");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.rare = 4;
		}

		public override void AddRecipes() {
			//TODO
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.DirtBlock, 1);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			bool history = false;
			List<ItemModel> localHistory = HistoryBookUI.MergeHistoryIntoInventory(this);
			if (localHistory.Count > 0) {
				for (int i = 0; i < localHistory.Count; i++) {
					ItemModel itemModel = localHistory[i];

					//Only show in the tooltip if there is a number assigned
					if (itemModel.SummonCount > 0) {
						if (!history) {
							history = true;
							tooltips.Add(new TooltipLine(mod, "History", "History:"));
						}

						tooltips.Add(new TooltipLine(mod, "ItemModel", itemModel.Name + ": " + itemModel.SummonCount) {
							overrideColor = itemModel.Active ? Color.White : Color.Red
						});
					}
				}
			}
			else {
				tooltips.Add(new TooltipLine(mod, "None", "No summon history specified"));
			}
		}

		public override bool UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				for (int i = 0; i < 1000; i++) {
					Projectile p = Main.projectile[i];
					if (p.active && p.owner == Main.myPlayer && p.minion) {
						p.Kill();
					}
				}

				var SAPlayer = player.GetModPlayer<SAPlayer>();
				SAPlayer.pendingCasts.Clear();
				foreach (var item in history) {
					for (int i = 0; i < item.SummonCount; i++) {
						SAPlayer.pendingCasts.Enqueue(new Tuple<int, int>(item.ItemType, 1));
					}
				}
			}
			return true;
		}
	}
}
