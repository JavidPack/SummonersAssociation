using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanTargetProjectile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Talisman Target Projectile");
		}

		public override void SetDefaults()
		{
			projectile.width = 100;
			projectile.height = 100;
			projectile.alpha = 255;// 55;
			projectile.timeLeft = 2;
			projectile.penetrate = -1;

			//	projectile.aiStyle = 20;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			//projectile.hide = true;
			projectile.ownerHitCheck = true;
			projectile.ignoreWater = true;
			projectile.damage = 0;
			//projectile.melee = true;
			//projectile.scale = 1.2f;
			//projectile.
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
		{
			width = 0;
			height = 0;
			return true;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			//byte alpha = (byte)(projectile.ai[0] > projectile.alpha ? projectile.alpha : projectile.ai[0]);
			return Color.White * ((255 - projectile.alpha) / 255f);
			//         Color a = Color.White;
			//a.A = (byte)projectile.alpha;
			//return a;
		}

		public override void AI()
		{
			//ErrorLogger.Log("d" + projectile.damage);

			if (Main.myPlayer == projectile.owner)
			{
				if (Main.player[projectile.owner].channel)
				{
					if (projectile.alpha <= 255)
						Main.player[projectile.owner].AddBuff(BuffID.Chilled, 300);
					if (projectile.alpha <= 245)
						Main.player[projectile.owner].AddBuff(BuffID.Darkness, 60);
					if (projectile.alpha <= 235)
						Main.player[projectile.owner].AddBuff(BuffID.Poisoned, 60);
					if (projectile.alpha <= 225)
						Main.player[projectile.owner].AddBuff(BuffID.Venom, 60);
					if (projectile.alpha <= 215)
						Main.player[projectile.owner].AddBuff(BuffID.Weak, 60);
					if (projectile.alpha <= 205)
						Main.player[projectile.owner].AddBuff(BuffID.Cursed, 100);
					if (projectile.alpha <= 195)
						Main.player[projectile.owner].AddBuff(BuffID.Slow, 60);
					if (projectile.alpha <= 185)
						Main.player[projectile.owner].AddBuff(BuffID.Confused, 60);
					if (projectile.alpha <= 175)
						Main.player[projectile.owner].AddBuff(BuffID.Blackout, 60);
					//if (projectile.alpha <= 165)
					//	Main.player[projectile.owner].AddBuff(BuffID.Obstructed, 60);
					if (projectile.alpha <= 155)
						Main.player[projectile.owner].AddBuff(BuffID.Electrified, 200);

					projectile.ai[0] += .01f;
					projectile.alpha -= 1;
					//	ErrorLogger.Log("a" + projectile.alpha);
					if (projectile.alpha < 155)
					{
						projectile.alpha = 155;
					}

					projectile.timeLeft = 2;
					//projectile.position.X = Main.mouseX + Main.screenPosition.X;
					//projectile.position.Y = Main.mouseY + Main.screenPosition.Y;
					projectile.Center = new Vector2(Main.mouseX + Main.screenPosition.X, Main.mouseY + Main.screenPosition.Y);

					if (Main.rand.Next(100) > projectile.alpha - 155)
					{
						int num14 = Dust.NewDust(new Vector2(projectile.position.X - 4f, projectile.position.Y - 4f), projectile.width + 8, projectile.height + 8, 202, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, default(Color), 1f);
						if (Main.rand.Next(2) == 0)
						{
							Main.dust[num14].scale = 1.5f;
						}
						Main.dust[num14].noGravity = true;
						Dust dustI = Main.dust[num14];
						dustI.velocity.X = dustI.velocity.X * 2f;
						dustI.velocity.Y = dustI.velocity.Y * 2f;
					}
				}
				else
				{
					//projectile.alpha += 1;
					//projectile.ai[1] = 1;
					projectile.Kill();
				}
			}
		}
	}
}