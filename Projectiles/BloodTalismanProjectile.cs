using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace SummonersAssociation.Projectiles
{
	//Spawns the "aura" in AI
	public class BloodTalismanProjectile : ModProjectile
	{
		public override void SetStaticDefaults() => DisplayName.SetDefault("Blood Talisman Projectile");

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.timeLeft = 18000;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
		}

		public override bool? CanCutTiles() => false;

		private void Show(Player player, Vector2 center) {
			Projectile.Center = center;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.spriteDirection = Projectile.direction;
			player.ChangeDir(Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
		}

		private void Aim(Player player, Vector2 center) {
			var aim = Vector2.Normalize(Main.MouseWorld - center);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			aim *= player.HeldItem.shootSpeed;

			if (aim != Projectile.velocity) {
				Projectile.netUpdate = true;
			}
			Projectile.velocity = aim;
		}

		private void SpawnAura(Player player) {
			int aura = ModContent.ProjectileType<BloodTalismanTargetProjectile>();
			if (player.ownedProjectileCounts[aura] <= 0) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Main.MouseWorld, Projectile.velocity, aura, 0, 0, player.whoAmI);
			}
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);

			Show(player, center);

			if (Main.myPlayer == Projectile.owner) {
				if (player.channel) {
					Aim(player, center);

					SpawnAura(player);
				}
				else {
					Projectile.Kill();
				}
			}
		}
	}
}
