using System.Collections.Generic;
using Terraria;

namespace SummonersAssociation.Models
{
	public class MinionModel
	{
		public int ItemID { get; set; }
		public int BuffID { get; set; }
		public List<int> ProjectileIDs { get; set; }
		public List<float> Slots { get; set; }

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
			Slots = new List<float> { slot };
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
			Slots = slots;
		}

		private List<float> GetSlotsPerProjectile() {
			var slots = new List<float>();
			foreach (int type in ProjectileIDs) {
				try {
					var proj = new Projectile();
					proj.SetDefaults(type);
					slots.Add(proj.minionSlots);
				}
				catch {
					// In case it gets called before PostSetupContent for whatever reason, default to 1
					slots.Add(1f);
				}
			}
			return slots;
		}
	}
}
