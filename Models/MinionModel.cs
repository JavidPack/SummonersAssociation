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

		public MinionModel(int itemID, int buffID, int projectileID) : this(itemID, buffID, new List<int> { projectileID }, null) {

		}

		public MinionModel(int itemID, int buffID, int projectileID, float slot) : this(itemID, buffID, new List<int> { projectileID }, new List<float> { slot }) {

		}

		public MinionModel(int itemID, int buffID, List<int> projectileIDs, List<float> slots = null) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = projectileIDs;
			Slots = slots ?? GetSlotsPerProjectile();
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
