using System.Collections.Generic;

namespace SummonersAssociation.Models
{
	public class MinionModel
	{
		public int ItemID { get; set; }
		public int BuffID { get; set; }
		public List<int> ProjectileIDs { get; set; }

		public MinionModel(int itemID, int buffID, List<int> projectileIDs) {
			ItemID = itemID;
			BuffID = buffID;
			ProjectileIDs = projectileIDs;
		}
	}
}