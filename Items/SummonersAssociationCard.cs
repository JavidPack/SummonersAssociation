using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		public override void SetDefaults()
		{
			item.name = "Summoners Association Card";
			item.width = 20;
			item.height = 20;
			item.maxStack = 1;
			item.toolTip = "Welcome to the Summoners Association";
			item.toolTip = "Displays Summoner-related information";
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
			recipe.AddCraftGroup(CraftGroup.Wood, 2);
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void UpdateInventory(Player player)
		{
			player.GetModPlayer<SummonersAssociationCardPlayer>(mod).SummonersAssociationCardInInventory = true;
		}
	}

	public class SummonersAssociationCardGlobalItem : GlobalItem
	{
		double time;
		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (time != Main.time)
			{
				time = Main.time;
				Player player = Main.player[Main.myPlayer];

				if (!player.GetModPlayer<SummonersAssociationCardPlayer>(mod).SummonersAssociationCardInInventory)
					return;

				if (!Main.ingameOptionsWindow && !Main.playerInventory && !Main.achievementsWindow)
				{
					float vanillaMinionSlots = 0;
					int xPosition;
					int yPosition;
					Color color;
					int buffsPerLine = 11;
					bool TwoLines = false;
					for (int b = 0; b < 22; ++b)
					{
						if (player.buffType[b] > 0)
						{
							if (b == 11)
							{
								TwoLines = true;
							}
							int buffID = player.buffType[b];
							xPosition = 32 + b * 38;
							yPosition = 76;
							if (b >= buffsPerLine)
							{
								xPosition = 32 + (b - buffsPerLine) * 38;
								yPosition += 50;
							}
							color = new Color(Main.buffAlpha[b], Main.buffAlpha[b], Main.buffAlpha[b], Main.buffAlpha[b]);

							bool isMinionBuff = false;
							int number = 0;

							if (buffID == BuffID.BabySlime)
							{
								number = player.ownedProjectileCounts[266];
								isMinionBuff = true;
                            }
							else if (buffID == BuffID.HornetMinion)
							{
								number = player.ownedProjectileCounts[373];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.ImpMinion)
							{
								number = player.ownedProjectileCounts[375];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.SpiderMinion)
							{
								number = player.ownedProjectileCounts[390] + player.ownedProjectileCounts[391] + player.ownedProjectileCounts[392];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.TwinEyesMinion)
							{
								number = player.ownedProjectileCounts[387] + player.ownedProjectileCounts[388];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.PirateMinion)
							{
								number = player.ownedProjectileCounts[393] + player.ownedProjectileCounts[394] + player.ownedProjectileCounts[395];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.Pygmies)
							{
								number = player.ownedProjectileCounts[191] + player.ownedProjectileCounts[192] + player.ownedProjectileCounts[193] + player.ownedProjectileCounts[194];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.UFOMinion)
							{
								number = player.ownedProjectileCounts[423];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.Ravens)
							{
								number = player.ownedProjectileCounts[317];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.SharknadoMinion)
							{
								number = player.ownedProjectileCounts[407];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.DeadlySphere)
							{
								number = player.ownedProjectileCounts[533];
								isMinionBuff = true;
							}
							else if (buffID == BuffID.StardustDragonMinion)
							{
								number = player.ownedProjectileCounts[626]; // and 627?
								isMinionBuff = true;
							}
							else if (buffID == BuffID.StardustMinion)
							{
								number = player.ownedProjectileCounts[613];
								isMinionBuff = true;
							}
							if (isMinionBuff)
							{
								string text2 = number + " / " + player.maxMinions;
								if (buffID == BuffID.TwinEyesMinion)
								{
									text2 = number + " / " + 2 * player.maxMinions;
									vanillaMinionSlots += number / 2f;
								}
								else
								{
									vanillaMinionSlots += number;
								}
								Main.spriteBatch.DrawString(Main.fontItemStack, text2, new Vector2((float)xPosition, (float)(yPosition + Main.buffTexture[buffID].Height)), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
							}
						}
					}
					// Count mod minions.
					color = new Color(Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1]);
					xPosition = 32;
					yPosition = 76 + 20;
					if (TwoLines)
					{
						yPosition += 50;
					}
					float otherMinions = 0;

					for (int j = 0; j < 1000; j++)
					{
						if (Main.projectile[j].active && Main.projectile[j].owner == player.whoAmI && Main.projectile[j].minion)
						{
							otherMinions += Main.projectile[j].minionSlots;
                        }
					}
					otherMinions = otherMinions - vanillaMinionSlots;
					if (otherMinions > 0)
					{
						string modMinionText = "Uncountable mod minions: " + otherMinions + " / " + player.maxMinions;
						Main.spriteBatch.DrawString(Main.fontItemStack, modMinionText, new Vector2((float)xPosition, (float)(yPosition + Main.buffTexture[1].Height)), color, 0.0f, new Vector2(), 0.8f, SpriteEffects.None, 0.0f);
					}
				}


				if (Main.playerInventory)
				{
					if (Main.EquipPage == 0)
					{
						int num8 = 6;
						int slot = num8;
						int num9 = Main.screenWidth - 64 - 28;
						int num11 = (int)((double)(174 + 0) + (double)(slot * 56) * (double)Main.inventoryScale);
						Vector2 vector2_1 = new Vector2((float)(num9 - 10 - 47 - 47 - 14), (float)num11 + (float)Main.inventoryBackTexture.Height * 0.5f);
						Main.spriteBatch.Draw(Main.buffTexture[150], vector2_1, new Microsoft.Xna.Framework.Rectangle?(), Microsoft.Xna.Framework.Color.White, 0.0f, Utils.Size(Main.buffTexture[150]) / 2f, Main.inventoryScale, SpriteEffects.None, 0.0f);
						Vector2 vector2_2 = Main.fontMouseText.MeasureString(player.maxMinions.ToString());
						Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, player.maxMinions.ToString(), vector2_1 - vector2_2 * 0.5f * Main.inventoryScale, Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, new Vector2(Main.inventoryScale), -1f, 2f);
						if (Utils.CenteredRectangle(vector2_1, Utils.Size(Main.buffTexture[150])).Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)))
						{
							player.mouseInterface = true;
							string str = "" + player.maxMinions + " Max Minions";
							if (!string.IsNullOrEmpty(str))
								Main.hoverItemName = str;
						}
					}
				}
			}
		}
	}
}