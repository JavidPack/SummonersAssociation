using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.Models
{
	// When expanding this, add handlers for the new data to the following:
	// * ProjModel.ConvertToDictionary
	// * ProjModel.FromDictionary
	// * SummonersAssociationIntegration.MinionModel.FromDictionary (in SummonersAssociationIntegrationExample.cs)
	// * ProjModel.FromProjID (if needed)
	/// <summary>
	/// Contains the projectile (ID) and any optional associated information
	/// </summary>
	/// <param name="ProjID">Mandatory</param>
	/// <param name="Slot">Optional</param>
	public record struct ProjModel(int ProjID, float Slot = 1f)
	{
		public override string ToString() => (ProjID < ProjectileID.Count ? Lang.GetProjectileName(ProjID) : ProjectileLoader.GetProjectile(ProjID).DisplayName).ToString();

		internal Dictionary<string, object> ConvertToDictionary(Version GetSupportedMinionsAPIVersion) {
			// We may want to allow different returns based on api version.
			//if (GetSupportedMinionsDictionaryAPIVersion == new Version(0, 4, 7)) {
			var dict = new Dictionary<string, object> {
				{ "ProjID", ProjID },
				{ "Slot", Slot },
			};

			return dict;
		}

		internal static ProjModel FromDictionary(Dictionary<string, object> dict, string error_itemMsg = null) {
			// Mandatory ProjID
			string projIDKey = "ProjID";
			if (!dict.TryGetValue(projIDKey, out object o_ProjID)) {
				string errorMsg = $"{nameof(ProjModel)} entry does not contain '{projIDKey}' key";
				if (error_itemMsg != null)
					throw new Exception(errorMsg + error_itemMsg);
				else
					throw new Exception(errorMsg);
			}
			int projID = Convert.ToInt32(o_ProjID);

			// Optional rest
			if (dict.TryGetValue("Slot", out object o_Slot)) {
				float slot = Convert.ToSingle(o_Slot);
				return new ProjModel(projID, slot);
			}
			else {
				// Fall back to the most basic constructor
				return FromProjID(projID);
			}
		}

		internal static ProjModel FromProjID(int projID) {
			try {
				var proj = new Projectile();
				proj.SetDefaults(projID);
				return new ProjModel(projID, proj.minionSlots);
			}
			catch {
				// In case it gets called before PostSetupContent for whatever reason, default to sensible values
				return new ProjModel(projID);
			}
		}
	}
}
