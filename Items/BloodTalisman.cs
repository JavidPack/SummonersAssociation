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
			Item.damage = 0;
			Item.shoot = ProjectileType<BloodTalismanProjectile>();
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useAnimation = 25;
			Item.useTime = 6;
			Item.shootSpeed = 15f;
			Item.knockBack = 4.5f;
			Item.width = 20;
			Item.height = 12;
			Item.rare = ItemRarityID.LightRed;
			Item.value = 108000;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemID.CrossNecklace).AddIngredient(ItemID.PhilosophersStone).AddTile(TileID.Chairs).AddTile(TileID.Tables).Register();
		}
	}
}
