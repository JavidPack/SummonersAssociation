using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanTargetProjectile : ModProjectile
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Blood Talisman Target Projectile");

		public override void SetDefaults() {
			projectile.width = 100;
			projectile.height = 100;
			projectile.alpha = 255;// 55;
			projectile.timeLeft = 2;
			projectile.penetrate = -1;
			//projectile.aiStyle = 20;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.ownerHitCheck = true;
			projectile.ignoreWater = true;
			projectile.damage = 0;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			width = 0;
			height = 0;
			return true;
		}

		public override Color? GetAlpha(Color lightColor) {
			//byte alpha = (byte)(projectile.ai[0] > projectile.alpha ? projectile.alpha : projectile.ai[0]);
			return Color.White * ((255 - projectile.alpha) / 255f);
			//         Color a = Color.White;
			//a.A = (byte)projectile.alpha;
			//return a;
		}

		public override void AI() {
			//ErrorLogger.Log("d" + projectile.damage);

			if (Main.myPlayer == projectile.owner) {
				Player player = Main.player[projectile.owner];
				if (player.channel) {
					if (projectile.alpha <= 255)
						player.AddBuff(BuffID.Chilled, 300);
					if (projectile.alpha <= 245)
						player.AddBuff(BuffID.Darkness, 60);
					if (projectile.alpha <= 235)
						player.AddBuff(BuffID.Poisoned, 60);
					if (projectile.alpha <= 225)
						player.AddBuff(BuffID.Venom, 60);
					if (projectile.alpha <= 215)
						player.AddBuff(BuffID.Weak, 60);
					if (projectile.alpha <= 205)
						player.AddBuff(BuffID.Cursed, 100);
					if (projectile.alpha <= 195)
						player.AddBuff(BuffID.Slow, 60);
					if (projectile.alpha <= 185)
						player.AddBuff(BuffID.Confused, 60);
					if (projectile.alpha <= 175)
						player.AddBuff(BuffID.Blackout, 60);
					//if (projectile.alpha <= 165)
					//	Main.player[projectile.owner].AddBuff(BuffID.Obstructed, 60);
					if (projectile.alpha <= 155)
						player.AddBuff(BuffID.Electrified, 200);

					projectile.ai[0] += .01f;
					projectile.alpha -= 1;
					//	ErrorLogger.Log("a" + projectile.alpha);
					if (projectile.alpha < 155) {
						projectile.alpha = 155;
					}

					projectile.timeLeft = 2;
					projectile.Center = Main.MouseWorld;

					if (Main.rand.Next(100) > projectile.alpha - 155) {
						int index = Dust.NewDust(new Vector2(projectile.position.X - 4f, projectile.position.Y - 4f), projectile.width + 8, projectile.height + 8, 202, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, default(Color), 1f);
						Dust dust = Main.dust[index];
						if (Main.rand.Next(2) == 0) {
							dust.scale = 1.5f;
						}
						dust.noGravity = true;
						dust.velocity.X *= 2f;
						dust.velocity.Y *= 2f;
					}
				}
				else {
					//projectile.alpha += 1;
					//projectile.ai[1] = 1;
					projectile.Kill();
				}
			}
		}
	}
}