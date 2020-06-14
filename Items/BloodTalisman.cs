using SummonersAssociation.Projectiles;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	//Spawns the "hold-out" projectile, which in turn spawns the "aura"
	public class BloodTalisman : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Sacrifice your lifeforce to greatly strengthen minions"
				+ "\n'Try not to fall into madness'");
		}

		public override void SetDefaults() {
			item.damage = 0;
			item.shoot = ProjectileType<BloodTalismanProjectile>();
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useAnimation = 25;
			item.useTime = 6;
			item.shootSpeed = 15f;
			item.knockBack = 4.5f;
			item.width = 20;
			item.height = 12;
			item.rare = ItemRarityID.LightRed;
			item.value = 108000;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.channel = true;
		}

		public override void AddRecipes() {
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.CrossNecklace);
			recipe.AddIngredient(ItemID.PhilosophersStone);
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
