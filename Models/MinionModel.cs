using System.Collections.Generic;
using Terraria;

namespace SummonersAssociation.Models
{
	public class MinionModel
	{
		public int ItemID { get; set; }
		public int BuffID { get; set; }
		public List<int> ProjectileIDs { get; set; }
		//Double because it's needed in Math.Floor and I cba to cast float to double every tick
		public List<double> Slots { get; set; }

		public override string ToString() => Terraria.ID.ItemID.GetUniqueKey(ItemID);

		public MinionModel(int itemID, int buffID, int projectileID) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = new List<int> { projectileID };
			Slots = GetSlotsPerProjectile();
		}

		public MinionModel(int itemID, int buffID, int projectileID, float slot) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = new List<int> { projectileID };
			Slots = new List<double> { slot };
		}

		public MinionModel(int itemID, int buffID, List<int> projectileIDs) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = projectileIDs;
			Slots = GetSlotsPerProjectile();
		}

		public MinionModel(int itemID, int buffID, List<int> projectileIDs, List<float> slots) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = projectileIDs;
			Slots = slots.ConvertAll(s => (double)s);
		}

		private List<double> GetSlotsPerProjectile() {
			var slots = new List<double>();
			foreach (int type in ProjectileIDs) {
				try {
					var proj = new Projectile();
					proj.SetDefaults(type);
					slots.Add(proj.minionSlots);
				}
				catch {
					// In case it gets called before PostSetupContent for whatever reason, default to 1
					slots.Add(1d);
				}
			}
			return slots;
		}
	}
}
