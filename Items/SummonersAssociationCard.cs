using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	public class SummonersAssociationCardPlayer : ModPlayer
	{
		public bool SummonersAssociationCardInInventory = false;

		public override void ResetEffects()
		{
			SummonersAssociationCardInInventory = false;
		}
	}

	public class SummonersAssociationCard : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Summoners Association Card");
			Tooltip.SetDefault("Welcome to the Summoners Association\nDisplays Summoner-related information");
		}

		public override void SetDefaults()
		{
			item.width = 20;
			item.height = 20;
			item.maxStack = 1;
			item.value = 500;
			item.rare = 2;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Gel, 10);
			recipe.AddRecipeGroup("Wood");
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UpdateInventory(Player player)
		{
			player.GetModPlayer<SummonersAssociationCardPlayer>().SummonersAssociationCardInInventory = true;
		}
	}
}