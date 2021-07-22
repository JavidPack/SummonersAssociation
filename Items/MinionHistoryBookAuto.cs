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
			Item.rare = ItemRarityID.Pink;
			Item.mana = 6;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemType<MinionHistoryBook>()).AddIngredient(ItemID.PixieDust, 10).AddTile(TileID.Bookcases).Register();
		}
	}
}
