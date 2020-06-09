using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanProjectile : ModProjectile
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Blood Talisman Projectile");

		public override void SetDefaults() {
			projectile.width = 30;
			projectile.height = 30;
			projectile.timeLeft = 2;
			projectile.penetrate = -1;
			//projectile.aiStyle = 20;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.hide = true;
			projectile.ownerHitCheck = true;
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];
			Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);
			if (Main.myPlayer == projectile.owner) {
				if (player.channel) {
					projectile.timeLeft = 2;

					float speed = player.HeldItem.shootSpeed * projectile.scale;
					Vector2 velo = Main.MouseWorld - center;
					velo.Normalize();
					velo *= speed;

					if (velo != projectile.velocity) {
						projectile.netUpdate = true;
					}
					projectile.velocity = velo;

					//Vector2 dir = Main.MouseWorld - Main.player[projectile.owner].Center;
					//dir *= (1f + (float)Main.rand.Next(-3, 4) * 0.01f);
					//int num14 = Dust.NewDust(Main.player[projectile.owner].Center, 0, 0, 218, dir.X * .05f, dir.Y * .05f, 0, default(Color), 1f);

				}
				else {
					projectile.Kill();
				}
			}
			if (projectile.velocity.X > 0f && player.direction != 1) {
				player.ChangeDir(1);
			}
			else if (projectile.velocity.X < 0f && player.direction != -1) {
				player.ChangeDir(-1);
			}
			projectile.spriteDirection = projectile.direction;
			//player.ChangeDir(projectile.direction);
			player.heldProj = projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			projectile.position.X = center.X - projectile.width / 2;
			projectile.position.Y = center.Y - projectile.height / 2;
			projectile.rotation = (float)(Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.5700000524520874);
			//projectile.rotation = 0;
			if (player.direction == 1) {
				player.itemRotation = (float)Math.Atan2(projectile.velocity.Y * projectile.direction, projectile.velocity.X * projectile.direction);
			}
			else {
				player.itemRotation = (float)Math.Atan2(projectile.velocity.Y * projectile.direction, projectile.velocity.X * projectile.direction);
			}
			projectile.velocity.X = projectile.velocity.X * (1f + (float)Main.rand.Next(-3, 4) * 0.01f);
		}
	}
}
