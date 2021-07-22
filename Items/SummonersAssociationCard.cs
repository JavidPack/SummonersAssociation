using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	public class SummonersAssociationCard : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Summoners Association Card");
			Tooltip.SetDefault("Welcome to the Summoners Association\nDisplays Summoner-related information");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 1;
			Item.value = 500;
			Item.rare = ItemRarityID.Green;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemID.Gel, 10).AddRecipeGroup("Wood").AddTile(TileID.Chairs).AddTile(TileID.Tables).Register();
		}

		public override void UpdateInventory(Player player)
			=> player.GetModPlayer<SummonersAssociationPlayer>().SummonersAssociationCardInInventory = true;
	}
}
