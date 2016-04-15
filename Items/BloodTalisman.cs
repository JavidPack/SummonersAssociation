using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SummonersAssociation.Items
{
	public class BloodTalisman : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Blood Talisman";
			//item.width = 20;
			//item.height = 20;
			//item.maxStack = 1;
			item.toolTip = "Sacrifice your lifeforce to greatly strengthen minions";
			item.toolTip2 = "Try not to fall into madness";
			//item.value = 500;
			//item.rare = 2;
			//item.useAnimation = 45;
			//item.useTime = 45;
			//	item.useStyle = 4;
			//item.channel = true;
			item.shoot = mod.ProjectileType("BloodTalismanProjectile");
			//item.noUseGraphic = true;

			item.useStyle = 5;
			item.useAnimation = 25;
			item.useTime = 6;
			item.shootSpeed = 15f;
			item.knockBack = 4.5f;
			item.width = 20;
			item.height = 12;
		//	item.damage = 33;
		//	item.axe = 20;
		//	item.useSound = 23;
			//item.shoot = 61;
			item.rare = 4;
			item.value = 108000;
			item.noMelee = true;
			item.noUseGraphic = true;
		//	item.melee = true;
			item.channel = true;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			//recipe.AddIngredient(ItemID.Wood);
			recipe.AddIngredient(ItemID.CrossNecklace);
			recipe.AddIngredient(ItemID.PhilosophersStone);
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			Projectile.NewProjectile(Main.mouseX + Main.screenPosition.X, Main.mouseY+ Main.screenPosition.Y, 0,0, mod.ProjectileType("BloodTalismanTargetProjectile"),0,0,player.whoAmI);
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
