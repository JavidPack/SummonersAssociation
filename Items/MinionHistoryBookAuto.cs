using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBookAuto : MinionHistoryBook
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Automatic Minion History Book");
			Tooltip.SetDefault("Left click to summon minions based on history"
				+ "\nRight click to open the UI"
				+ "\nScroll wheel over the item icons to adjust the summon count"
				+ "\nAutomatically summons minions when you respawn");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.rare = ItemRarityID.Pink;
			item.mana = 6;
		}

		public override void AddRecipes() {
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<MinionHistoryBook>());
			recipe.AddIngredient(ItemID.PixieDust, 10);
			recipe.AddTile(TileID.Bookcases);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
