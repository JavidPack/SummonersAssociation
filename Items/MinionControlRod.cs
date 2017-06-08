using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Items
{
	public class MinionControlRod : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Minion Control Rod");
			Tooltip.SetDefault("Dominate and direct your minions.");
		}

		public override void SetDefaults()
		{
			item.width = 48;
			item.height = 48;
			item.maxStack = 1;
			item.value = 500;
			item.rare = 2;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.useTurn = true;
			item.UseSound = SoundID.Item6;
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("SummonersAssociation:MinionStaffs");
			recipe.AddRecipeGroup("SummonersAssociation:MagicMirrors");
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UseStyle(Player player)
		{
			player.itemLocation.X += player.direction * -16;
			player.itemLocation.Y += -16;

			if (player.itemTime == 0)
			{
				player.itemTime = item.useTime;
			}
			if (player.itemTime == item.useTime / 2)
			{
				//for (int i = 0; i < 300; i++)
				//{
				//	Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, default(Color), 1.1f);
				//}
				bool teleAny = false;
				Vector2 mousePoint = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);
				for (int j = 0; j < 1000; j++)
				{
					if (Main.projectile[j].active && Main.projectile[j].owner == player.whoAmI)
					{
						if (Main.projectile[j].minion)
						{
							//Main.projectile[j].position.X = (float)Main.mouseX + Main.screenPosition.X;
							//Main.projectile[j].position.Y = (float)Main.mouseY + Main.screenPosition.Y;
							for (int i = 0; i < 60; i++)
							{
								Dust.NewDust(Main.projectile[j].position, player.width, player.height, 14, 0f, 0f, 150, default(Color), 1.1f);
							}
							Main.projectile[j].position = mousePoint;
							teleAny = true;
						}
					}
				}
				if (teleAny)
					for (int i = 0; i < 60; i++)
					{
						Dust.NewDust(mousePoint, player.width, player.height, 15, 0f, 0f, 150, default(Color), 1.1f);
						//	Dust.NewDust(player.position, player.width, player.height, 15, 0f, 0f, 150, default(Color), 1.1f);
					}
			}
		}
	}
}
