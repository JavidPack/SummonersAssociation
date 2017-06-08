using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace SummonersAssociation.Projectiles
{
	public class BloodTalismanGlobalProjectile : GlobalProjectile
	{

		//public override void SetDefaults(Projectile projectile)
		//{
		//	// hornet						imp						optic					pygmy						ufo						sharkba					minicell	
		//	if (projectile.type == ProjectileID.HornetStinger|| projectile.type == ProjectileID.MiniRetinaLaser|| projectile.type == ProjectileID.ImpFireball || projectile.type == ProjectileID.PygmySpear || projectile.type == ProjectileID.UFOLaser || projectile.type == ProjectileID.MiniSharkron || projectile.type == ProjectileID.StardustCellMinionShot)
		//	{
		//		Projectile p = null;
		//		//if pro owner is channel and 
		//		for (int j = 0; j < 1000; j++)
		//		{
		//			if (Main.projectile[j].active && Main.projectile[j].owner == projectile.owner)
		//			{
		//				if (Main.projectile[j].type == mod.ProjectileType("BloodTalismanTargetProjectile"))
		//				{
		//					p = Main.projectile[j];
		//					break;
		//				}
		//			}
		//		}
		//		if (p != null)
		//		{
		//			if (Vector2.Distance(p.position, projectile.position) < 100)
		//			{
		//				projectile.damage *= 100;
		//				projectile.knockBack *= 100;
		//			}
		//		}
		//	}
		//}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			// if intersecting with bloodtalismantarget
			//projectile.type == ProjectileID.HornetStinger || projectile.type == ProjectileID.MiniRetinaLaser || projectile.type == ProjectileID.ImpFireball || projectile.type == ProjectileID.PygmySpear || projectile.type == ProjectileID.UFOLaser || projectile.type == ProjectileID.MiniSharkron || projectile.type == ProjectileID.StardustCellMinionShot
			if (projectile.minion || ProjectileID.Sets.MinionShot[projectile.type])
			{
				Projectile p = null;
				//if pro owner is channel and 
				for (int j = 0; j < 1000; j++)
				{
					if (Main.projectile[j].active && Main.projectile[j].owner == projectile.owner)
					{
						if (Main.projectile[j].type == mod.ProjectileType<Projectiles.BloodTalismanTargetProjectile>()){
							p = Main.projectile[j];
							break;
						}
                    }
				}
				if (p != null)
				{
					if(Vector2.Distance(p.Center , target.Center/*projectile.Center*/) < 50)
					{
						// 155 max
						// 10x damage
						if(p.alpha == 155)
						{
							damage *= 10;
							knockback *= 2.5f; // 1.5
						}
						// 5 to almost 10x
						else if(p.alpha < 170)
						{
							damage = (int)((5 + ((170 - p.alpha) / 5f)) * damage); // 5 to 10
							knockback *= (1.75f + .75f*((195 - p.alpha) / 25f)); // .75 to 1.5
						}
						//
						else if (p.alpha < 195)
						{
							damage = (int)((2.5 + ((195 - p.alpha) / 10f)) * damage);
							knockback *= ((1.3f + .3f*((195 - p.alpha) / 25f)) );
						}
						else if (p.alpha < 220)
						{
							damage = (int)((1.25 + ((220 - p.alpha) / 20f)) * damage);
							knockback *= ((1.15f + .15f*((195 - p.alpha) / 25f)));
						}
						else /*if (p.alpha < 255)*/
						{
							damage = (int)(1.1f * damage);
							knockback *= 1.05f;
						}
					}
				}
			}
		}
	}
}
