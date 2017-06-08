using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SummonersAssociation.Items;
using ReLogic.Graphics;

namespace SummonersAssociation
{
	public class SummonersAssociation : Mod
	{
		public SummonersAssociation()
		{
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Lang.misc[37].Value + " Minion Staff", new int[]
			{
				ItemID.SlimeStaff,
				ItemID.ImpStaff,
				ItemID.HornetStaff,
				ItemID.SpiderStaff,
				ItemID.OpticStaff,
				ItemID.PirateStaff,
				ItemID.PygmyStaff,
				ItemID.XenoStaff,
				ItemID.RavenStaff,
				ItemID.TempestStaff,
				ItemID.DeadlySphereStaff,
				ItemID.StardustCellStaff,
				ItemID.StardustDragonStaff
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MinionStaffs", group);

			group = new RecipeGroup(() => Lang.misc[37].Value + " Magic Mirror", new int[]
			{
				ItemID.IceMirror,
				ItemID.MagicMirror
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
		}

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			Player player = Main.player[Main.myPlayer];

			if (!player.GetModPlayer<SummonersAssociationCardPlayer>().SummonersAssociationCardInInventory)
				return;

			if (!Main.ingameOptionsWindow && !Main.playerInventory/* && !Main.achievementsWindow*/)
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
				//color = new Color(Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1], Main.buffAlpha[1]);
				color = new Color(.4f, .4f, .4f, .4f);
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

			float inventoryScale = 0.85f;

			if (Main.playerInventory)
			{
				if (Main.EquipPage == 0)
				{
					int mH = 0;
					if (Main.mapEnabled)
					{
						if (!Main.mapFullscreen && Main.mapStyle == 1)
						{
							mH = 256;
						}
						if (mH + 600 > Main.screenHeight)
						{
							mH = Main.screenHeight - 600;
						}
					}

					int num8 = 6;
					int slot = num8;
					int num9 = Main.screenWidth - 64 - 28;

					int num11 = mH + (int)((double)(174 + 0) + (double)(slot * 56) * (double)inventoryScale);
					Vector2 vector2_1 = new Vector2((float)(num9 - 10 - 47 - 47 - 14), (float)num11 + (float)Main.inventoryBackTexture.Height * 0.5f);
					Main.spriteBatch.Draw(Main.buffTexture[150], vector2_1, new Microsoft.Xna.Framework.Rectangle?(), Color.White, 0.0f, Utils.Size(Main.buffTexture[150]) / 2f, inventoryScale, SpriteEffects.None, 0.0f);
					Vector2 vector2_2 = Main.fontMouseText.MeasureString(player.maxMinions.ToString());
					Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, player.maxMinions.ToString(), vector2_1 - vector2_2 * 0.5f * inventoryScale, Color.White, 0.0f, Vector2.Zero, new Vector2(inventoryScale), -1f, 2f);
					if (Utils.CenteredRectangle(vector2_1, Utils.Size(Main.buffTexture[150])).Contains(new Point(Main.mouseX, Main.mouseY)))
					{
						player.mouseInterface = true;
						string str = "" + player.maxMinions + " Max Minions";
						if (!string.IsNullOrEmpty(str))
							Main.hoverItemName = str;
					}
				}
			}
		}

		// TODO: summonersAssociation.Call("Buff->Projectile", BuffType("CoolMinionBuff"), ProjectileType("CoolMinionProjectile")); style call.
		//public override object Call(params object[] args)
		//{
		//	return base.Call(args);
		//}
	}
}
