using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanGlobalProjectile : GlobalProjectile
	{
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			//If intersecting with BloodTalismanTargetProjectile, increase damage and knockback
			if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type]) {
				Projectile talismanProj = null;
				int talismanType = ProjectileType<BloodTalismanTargetProjectile>();
				for (int j = 0; j < Main.maxProjectiles; j++) {
					Projectile p = Main.projectile[j];
					if (p.active && p.owner == projectile.owner && p.type == talismanType) {
						talismanProj = p;
						break;
					}
				}

				if (talismanProj != null) {
					var talisman = talismanProj.ModProjectile as BloodTalismanTargetProjectile;
					int alpha = talisman.alphaTimer;
					if (talismanProj.DistanceSQ(target.Center) < 50 * 50) {
						// 155 max
						// 10x damage
						if (alpha <= BloodTalismanTargetProjectile.alphaFinal) {
							modifiers.SourceDamage += 10;
							modifiers.Knockback += 2.5f; // 1.5
						}
						// 5 to almost 10x
						else if (alpha < 170) {
							modifiers.SourceDamage += 5 + ((170 - alpha) / 5f); // 5 to 10
							modifiers.Knockback += 1.75f + .75f * ((195 - alpha) / 25f); // .75 to 1.5
						}
						//
						else if (alpha < 195) {
							modifiers.SourceDamage += 2.5f + ((195 - alpha) / 10f);
							modifiers.Knockback += 1.3f + .3f * ((195 - alpha) / 25f);
						}
						else if (alpha < 220) {
							modifiers.SourceDamage += 1.25f + ((220 - alpha) / 20f);
							modifiers.Knockback += 1.15f + .15f * ((195 - alpha) / 25f);
						}
						else /*if (alpha < 255)*/
						{
							modifiers.SourceDamage += 1.1f;
							modifiers.Knockback += 1.05f;
						}

						//Offset previously used *= calculations manually to match += syntax since I can't be bothered to refactor this mess
						modifiers.SourceDamage -= 1;
						modifiers.Knockback -= 1f;
					}
				}
			}
		}
	}
}
