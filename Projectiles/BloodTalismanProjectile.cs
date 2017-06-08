using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Talisman Projectile");
		}

		public override void SetDefaults()
		{
			projectile.width = 30;
			projectile.height = 30;
			//projectile.alpha = 255;
			projectile.timeLeft = 2;
			projectile.penetrate = -1;

			//	projectile.aiStyle = 20;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.hide = true;
			projectile.ownerHitCheck = true;
			//	projectile.melee = true;
			//projectile.scale = 1.2f;
		}
		public override void AI()
		{

			Vector2 vector22 = Main.player[projectile.owner].RotatedRelativePoint(Main.player[projectile.owner].MountedCenter, true);
			if (Main.myPlayer == projectile.owner)
			{
				if (Main.player[projectile.owner].channel)
				{


					projectile.timeLeft = 2;
					float num263 = Main.player[projectile.owner].inventory[Main.player[projectile.owner].selectedItem].shootSpeed * projectile.scale;
					Vector2 vector23 = vector22;
					float num264 = (float)Main.mouseX + Main.screenPosition.X - vector23.X;
					float num265 = (float)Main.mouseY + Main.screenPosition.Y - vector23.Y;
					if (Main.player[projectile.owner].gravDir == -1f)
					{
						num265 = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector23.Y;
					}
					float num266 = (float)Math.Sqrt((double)(num264 * num264 + num265 * num265));
					//num266 = (float)Math.Sqrt((double)(num264 * num264 + num265 * num265));
					num266 = num263 / num266;
					num264 *= num266;
					num265 *= num266;
					if (num264 != projectile.velocity.X || num265 != projectile.velocity.Y)
					{
						projectile.netUpdate = true;
					}
					projectile.velocity.X = num264;
					projectile.velocity.Y = num265;
					Vector2 dir = Main.MouseWorld - Main.player[projectile.owner].Center;
					dir *= (1f + (float)Main.rand.Next(-3, 4) * 0.01f);
					int num14 = Dust.NewDust(Main.player[projectile.owner].Center, 0, 0, 218, dir.X * .05f, dir.Y * .05f, 0, default(Color), 1f);

				}
				else
				{
					projectile.Kill();
				}
			}
			if (projectile.velocity.X > 0f)
			{
				Main.player[projectile.owner].ChangeDir(1);
			}
			else if (projectile.velocity.X < 0f)
			{
				Main.player[projectile.owner].ChangeDir(-1);
			}
			projectile.spriteDirection = projectile.direction;
			Main.player[projectile.owner].ChangeDir(projectile.direction);
			Main.player[projectile.owner].heldProj = projectile.whoAmI;
			Main.player[projectile.owner].itemTime = 2;
			Main.player[projectile.owner].itemAnimation = 2;
			projectile.position.X = vector22.X - (float)(projectile.width / 2);
			projectile.position.Y = vector22.Y - (float)(projectile.height / 2);
			projectile.rotation = (float)(Math.Atan2((double)projectile.velocity.Y, (double)projectile.velocity.X) + 1.5700000524520874);
			//projectile.rotation = 0;
			if (Main.player[projectile.owner].direction == 1)
			{
				Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));
			}
			else
			{
				Main.player[projectile.owner].itemRotation = (float)Math.Atan2((double)(projectile.velocity.Y * (float)projectile.direction), (double)(projectile.velocity.X * (float)projectile.direction));
			}
			projectile.velocity.X = projectile.velocity.X * (1f + (float)Main.rand.Next(-3, 4) * 0.01f);
		}
	}
}
