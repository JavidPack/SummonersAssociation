using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBookAuto : MinionHistoryBook
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Automatic Minion History Book");
			Tooltip.SetDefault("Left click to summon minions based on history"
				+ "\nRight click to open the UI"
				+ "\nScroll whell over the item icons to adjust the summon count"
				+ "\nAutomatically summons minions when you respawn");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.rare = 5;
			item.mana = 6;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(mod.ItemType<MinionHistoryBook>());
			recipe.AddIngredient(ItemID.LihzahrdPowerCell, 2);
			recipe.AddTile(TileID.LihzahrdAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
