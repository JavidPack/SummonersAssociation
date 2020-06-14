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
			projectile.width = 30;
			projectile.height = 30;
			projectile.timeLeft = 18000;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.penetrate = -1;
			projectile.ignoreWater = true;
			projectile.tileCollide = false;
		}

		public override bool? CanCutTiles() => false;

		private void Show(Player player, Vector2 center) {
			projectile.Center = center;
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			projectile.spriteDirection = projectile.direction;
			player.ChangeDir(projectile.direction);
			player.heldProj = projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.itemRotation = (projectile.velocity * projectile.direction).ToRotation();
		}

		private void Aim(Player player, Vector2 center) {
			var aim = Vector2.Normalize(Main.MouseWorld - center);
			if (aim.HasNaNs()) {
				aim = -Vector2.UnitY;
			}

			aim *= player.HeldItem.shootSpeed;

			if (aim != projectile.velocity) {
				projectile.netUpdate = true;
			}
			projectile.velocity = aim;
		}

		private void SpawnAura(Player player) {
			int aura = ModContent.ProjectileType<BloodTalismanTargetProjectile>();
			if (player.ownedProjectileCounts[aura] <= 0) {
				Projectile.NewProjectile(Main.MouseWorld, projectile.velocity, aura, 0, 0, player.whoAmI);
			}
		}

		public override void AI() {
			Player player = Main.player[projectile.owner];
			Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);

			Show(player, center);

			if (Main.myPlayer == projectile.owner) {
				if (player.channel) {
					Aim(player, center);

					SpawnAura(player);
				}
				else {
					projectile.Kill();
				}
			}
		}
	}
}
