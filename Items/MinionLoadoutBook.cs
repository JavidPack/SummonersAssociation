using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	[LegacyName("MinionHistoryBook")]
	public class MinionLoadoutBook : MinionLoadoutBookSimple
	{
		public static LocalizedText NoLoadoutText { get; private set; }
		public static LocalizedText ExpectedManaCostText { get; private set; }

		public static LocalizedText LoadoutHeaderText { get; private set; }
		public static LocalizedText SummonsPerUseText { get; private set; }

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			//Localizations here should be "static" since this class is inherited from
			string category = $"{GetInstance<MinionLoadoutBook>().LocalizationCategory}.{nameof(MinionLoadoutBook)}.";
			NoLoadoutText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}NoLoadout"));
			ExpectedManaCostText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}ExpectedManaCost"));

			LoadoutHeaderText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}LoadoutHeader"));
			SummonsPerUseText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}SummonsPerUse"));
		}

		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.LightRed;
			Item.mana = 4;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemType<MinionLoadoutBookSimple>()).AddIngredient(ItemID.Bone, 10).AddIngredient(ItemID.JungleSpores, 5).AddIngredient(ItemID.SummoningPotion, 5).AddTile(TileID.Bookcases).Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			GetLoadoutInfo(out List<TooltipLine> loadoutTooltips, out int totalManaCost);
			bool hasLoadout = loadoutTooltips.Count > 0;

			//Append loadout stuff to the end
			if (!hasLoadout) {
				tooltips.Add(new TooltipLine(Mod, "None", NoLoadoutText.ToString()));
			}
			else {
				if (totalManaCost > 0) {
					int manaCostIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name == "UseMana");
					if (manaCostIndex > -1) {
						tooltips.Insert(manaCostIndex + 1, new TooltipLine(Mod, "LoadoutManaCost", ExpectedManaCostText.Format(totalManaCost)));
					}
				}

				tooltips.AddRange(loadoutTooltips);
			}
		}

		private void GetLoadoutInfo(out List<TooltipLine> loadoutTooltips, out int totalManaCost) {
			loadoutTooltips = new List<TooltipLine>();
			totalManaCost = 0;

			bool loadout = false;
			List<ItemModel> localLoadout = LoadoutBookUI.MergeLoadoutIntoInventory(this);
			if (localLoadout.Count > 0) {
				for (int i = 0; i < localLoadout.Count; i++) {
					ItemModel itemModel = localLoadout[i];

					//Only show in the tooltip if there is a number assigned
					if (itemModel.SummonCount > 0) {
						if (!loadout) {
							loadout = true;
							loadoutTooltips.Add(new TooltipLine(Mod, "Loadout", LoadoutHeaderText.ToString()));
						}

						totalManaCost += itemModel.SummonCount * ContentSamples.ItemsByType[itemModel.ItemType].mana; //Rough estimate, could've added this to ItemModel itself, but mana changes through ModifyManaCost won't get detected through this either way

						loadoutTooltips.Add(new TooltipLine(Mod, $"ItemModel_{itemModel.Name}", SummonsPerUseText.Format(itemModel.Name, itemModel.SummonCount)) {
							OverrideColor = itemModel.Active ? Color.White : Color.Red
						});
					}
				}
			}
		}
	}
}
