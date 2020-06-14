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
			projectile.alpha = 255;
			projectile.timeLeft = 18000;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.ownerHitCheck = true;
			projectile.ignoreWater = true;
		}

		public override Color? GetAlpha(Color lightColor) => Color.White * projectile.Opacity;

		public override bool? CanCutTiles() => false;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
			width = 0;
			height = 0;
			return true;
		}

		public const int alphaFinal = 155;

		public int alphaTimer = 255;

		private void Visuals() {
			if (Main.rand.Next(100) > alphaTimer - 155) {
				int index = Dust.NewDust(new Vector2(projectile.position.X - 4f, projectile.position.Y - 4f), projectile.width + 8, projectile.height + 8, 202, projectile.velocity.X * 0.2f, projectile.velocity.Y * 0.2f, 0, default(Color), 1f);
				Dust dust = Main.dust[index];
				if (Main.rand.NextBool(2)) {
					dust.scale = 1.5f;
				}
				dust.noGravity = true;
				dust.velocity *= 2f;
			}
		}

		private void UpdateAlpha() {
			alphaTimer--;
			if (alphaTimer < alphaFinal) {
				alphaTimer = alphaFinal;
			}
			projectile.alpha = alphaTimer;
		}

		private void GiveDebuffs(Player player) {
			if (alphaTimer <= 255)
				player.AddBuff(BuffID.Chilled, 300);
			if (alphaTimer <= 245)
				player.AddBuff(BuffID.Darkness, 60);
			if (alphaTimer <= 235)
				player.AddBuff(BuffID.Poisoned, 60);
			if (alphaTimer <= 225)
				player.AddBuff(BuffID.Venom, 60);
			if (alphaTimer <= 215)
				player.AddBuff(BuffID.Weak, 60);
			if (alphaTimer <= 205)
				player.AddBuff(BuffID.Cursed, 100);
			if (alphaTimer <= 195)
				player.AddBuff(BuffID.Slow, 60);
			if (alphaTimer <= 185)
				player.AddBuff(BuffID.Confused, 60);
			if (alphaTimer <= 175)
				player.AddBuff(BuffID.Blackout, 60);
			//if (alphaTimer <= 165)
			//	Main.player[projectile.owner].AddBuff(BuffID.Obstructed, 60);
			if (alphaTimer <= alphaFinal)
				player.AddBuff(BuffID.Electrified, 200);
		}

		public override void AI() {
			UpdateAlpha();

			Visuals();

			if (Main.myPlayer == projectile.owner) {
				Player player = Main.player[projectile.owner];
				if (player.channel) {
					GiveDebuffs(player);

					projectile.timeLeft = 2;
					float maxDistance = 32f; //This also sets the maximum speed the projectile can reach while following the cursor
					Vector2 vectorToCursor = Main.MouseWorld - projectile.Center;
					float distanceToCursor = vectorToCursor.Length();

					//Here we can see that the speed of the projectile depends on the distance to the cursor
					if (distanceToCursor > maxDistance) {
						distanceToCursor = maxDistance / distanceToCursor;
						vectorToCursor *= distanceToCursor;
					}

					int velocityXBy1000 = (int)(vectorToCursor.X * 1000f);
					int oldVelocityXBy1000 = (int)(projectile.velocity.X * 1000f);
					int velocityYBy1000 = (int)(vectorToCursor.Y * 1000f);
					int oldVelocityYBy1000 = (int)(projectile.velocity.Y * 1000f);

					//This code checks if the previous velocity of the projectile is different enough from its new velocity, and if it is, syncs it with the server and the other clients in MP
					//We previously multiplied the speed by 1000, then casted it to int, this is to reduce its precision and prevent the speed from being synced too much
					if (velocityXBy1000 != oldVelocityXBy1000 || velocityYBy1000 != oldVelocityYBy1000) {
						projectile.netUpdate = true;
					}

					projectile.velocity = vectorToCursor;
				}
				else {
					projectile.Kill();
				}
			}
		}
	}
}