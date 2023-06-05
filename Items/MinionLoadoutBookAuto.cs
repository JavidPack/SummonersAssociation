using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	[LegacyName("MinionHistoryBookAuto")]
	public class MinionLoadoutBookAuto : MinionLoadoutBook
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.Pink;
			Item.mana = 6;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ModContent.ItemType<MinionLoadoutBook>()).AddIngredient(ItemID.PixieDust, 10).AddTile(TileID.Bookcases).Register();
		}
	}
}
