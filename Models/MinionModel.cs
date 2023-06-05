using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace SummonersAssociation.Models
{
	public class MinionModel
	{
		public int ItemID { get; init; }
		public int BuffID { get; init; }

		public List<ProjModel> ProjData { get; init; }

		public override string ToString() => ItemID < Terraria.ID.ItemID.Count ? Lang.GetItemNameValue(ItemID) : ItemLoader.GetItem(ItemID).DisplayName.ToString();

		// The most basic constructor (no optional values)
		public MinionModel(int itemID, int buffID, int projectileID) : this(itemID, buffID, ProjModel.FromProjID(projectileID)) {

		}

		// The second-most basic constructor (no optional values)
		public MinionModel(int itemID, int buffID, List<int> projectileIDs) : this(itemID, buffID, GetProjDataFromProjIDs(projectileIDs)) {

		}

		// Main constructor (convenience)
		public MinionModel(int itemID, int buffID, ProjModel projModel) : this(itemID, buffID, new List<ProjModel> { projModel }) {

		}

		// Main constructor
		public MinionModel(int itemID, int buffID, List<ProjModel> projData) {
			ItemID = itemID;
			BuffID = buffID;
			ProjData = projData;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="other">To copy</param>
		public MinionModel(MinionModel other) {
			ItemID = other.ItemID;
			BuffID = other.BuffID;
			ProjData = other.ProjData;
		}

		internal static List<ProjModel> GetProjDataFromProjIDs(List<int> projIDs) {
			var list = new List<ProjModel>();
			foreach (int type in projIDs) {
				list.Add(ProjModel.FromProjID(type));
			}
			return list;
		}

		public bool ContainsProjID(int projID) => ProjData.Any(d => d.ProjID == projID);

		internal Dictionary<string, object> ConvertToDictionary(Version GetSupportedMinionsAPIVersion) {
			// We may want to allow different returns based on api version.
			//if (GetSupportedMinionsDictionaryAPIVersion == new Version(0, 4, 7)) {
			var dict = new Dictionary<string, object> {
				{ "ItemID", ItemID },
				{ "BuffID", BuffID },
				{ "ProjData", ProjData.Select(d => d.ConvertToDictionary(GetSupportedMinionsAPIVersion)).ToList() },
			};

			return dict;
		}
	}
}
