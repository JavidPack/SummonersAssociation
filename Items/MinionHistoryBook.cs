using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBook : MinionHistoryBookSimple
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion History Book");
			Tooltip.SetDefault("Left click to summon minions based on history"
				+ "\nRight click to open an UI"
				+ "\nScroll wheel over the item icons to adjust the summon count");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.LightRed;
			Item.mana = 4;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemType<MinionHistoryBookSimple>()).AddIngredient(ItemID.Bone, 10).AddIngredient(ItemID.JungleSpores, 5).AddIngredient(ItemID.SummoningPotion, 5).AddTile(TileID.Bookcases).Register();
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
							tooltips.Add(new TooltipLine(Mod, "History", "History:"));
						}

						tooltips.Add(new TooltipLine(Mod, "ItemModel", itemModel.Name + ": " + itemModel.SummonCount) {
							overrideColor = itemModel.Active ? Color.White : Color.Red
						});
					}
				}
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "None", "No summon history specified"));
			}
		}
	}
}
