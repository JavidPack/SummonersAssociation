using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBookAuto : MinionHistoryBook
	{
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
