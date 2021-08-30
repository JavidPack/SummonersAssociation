using System;
using System.Collections.Generic;
using Terraria;

namespace SummonersAssociation.Models
{
	public class MinionModel
	{
		public int ItemID { get; private set; }
		public int BuffID { get; private set; }
		public List<int> ProjectileIDs { get; private set; }
		public List<float> Slots { get; private set; }

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

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other">MinionModel to copy</param>
		public MinionModel(MinionModel other) {
			ItemID = other.ItemID;
			BuffID = other.BuffID;
			ProjectileIDs = other.ProjectileIDs;
			Slots = other.Slots;
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

		internal Dictionary<string, object> ConvertToDictionary(Version GetSupportedMinionsAPIVersion) {
			// We may want to allow different returns based on api version.
			//if (GetSupportedMinionsDictionaryAPIVersion == new Version(0, 4, 7)) {
			var dict = new Dictionary<string, object> {
				{ "ItemID", ItemID },
				{ "BuffID", BuffID },
				{ "ProjectileIDs", ProjectileIDs },
				{ "Slots", Slots },
			};

			return dict;
		}
	}
}
