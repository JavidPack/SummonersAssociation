using Microsoft.Xna.Framework;
using SummonersAssociation.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ID.ItemID;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Items
{
	public class BloodTalisman : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Sacrifice your lifeforce to greatly strengthen minions"
				+ "\nTry not to fall into madness");
		}

		public override void SetDefaults() {
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
			//recipe.AddIngredient(ItemID.Wood);
			recipe.AddIngredient(CrossNecklace);
			recipe.AddIngredient(ItemID.PhilosophersStone);
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			Projectile.NewProjectile(Main.MouseWorld, Vector2.Zero, ProjectileType<BloodTalismanTargetProjectile>(), 0, 0, player.whoAmI);
			//return false;
			return true;
		}

		//public override bool UseItem(Player player)
		//{
		//	if (player.channel)
		//	{
		//		player.AddBuff(BuffID.Cursed, 2);
		//		player.AddBuff(BuffID.Darkness, 2);
		//	}
		//          return true;
		//}
	}
}
