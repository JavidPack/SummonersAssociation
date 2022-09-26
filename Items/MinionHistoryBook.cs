using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBook : MinionHistoryBookSimple
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
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
			GetHistoryInfo(out List<TooltipLine> historyTooltips, out int totalManaCost);
			bool hasHistory = historyTooltips.Count > 0;

			//Append history stuff to the end
			if (!hasHistory) {
				tooltips.Add(new TooltipLine(Mod, "None", "No summon history specified"));
			}
			else {
				if (totalManaCost > 0) {
					int manaCostIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name == "UseMana");
					if (manaCostIndex > -1) {
						tooltips.Insert(manaCostIndex + 1, new TooltipLine(Mod, "HistoryManaCost", $"Expected {totalManaCost} additional mana from current loadout"));
					}
				}

				tooltips.AddRange(historyTooltips);
			}
		}

		private void GetHistoryInfo(out List<TooltipLine> historyTooltips, out int totalManaCost) {
			historyTooltips = new List<TooltipLine>();
			totalManaCost = 0;

			bool history = false;
			List<ItemModel> localHistory = HistoryBookUI.MergeHistoryIntoInventory(this);
			if (localHistory.Count > 0) {
				for (int i = 0; i < localHistory.Count; i++) {
					ItemModel itemModel = localHistory[i];

					//Only show in the tooltip if there is a number assigned
					if (itemModel.SummonCount > 0) {
						if (!history) {
							history = true;
							historyTooltips.Add(new TooltipLine(Mod, "History", "History:"));
						}

						totalManaCost += itemModel.SummonCount * ContentSamples.ItemsByType[itemModel.ItemType].mana; //Rough estimate, could've added this to ItemModel itself, but mana changes through ModifyManaCost won't get detected through this either way

						historyTooltips.Add(new TooltipLine(Mod, $"ItemModel_{itemModel.Name}", $"{itemModel.Name}: {itemModel.SummonCount}") {
							OverrideColor = itemModel.Active ? Color.White : Color.Red
						});
					}
				}
			}
		}
	}
}
