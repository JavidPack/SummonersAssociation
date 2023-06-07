/*
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace <YourModNameHere>
{
	// This class provides an example of advanced Summoners' Association integration utilizing the "GetSupportedMinions" Mod.Call that other Mods can copy into their mod's source code.
	// By copying this class into your mod, you can access Summoners' Association minion data reliably and with type safety without requiring a strong dependency.
	public class SummonersAssociationIntegration : ModSystem
	{
		// Summoners' Association might add new features, so a version is passed into GetSupportedMinions. 
		// If a new version of the GetSupportedMinions Call is implemented, find this class in the Summoners' Association Github once again and replace this version with the new version: https://github.com/JavidPack/SummonersAssociation/blob/master/SummonersAssociationIntegrationExample.cs
		private static readonly Version SummonersAssociationAPIVersion = new Version(0, 5); // Do not change this yourself.

		public record struct ProjModel(int ProjID, float Slot);

		public class MinionModel
		{
			public int ItemID { get; set; } // The item ID associated with the minion(s)
			public int BuffID { get; set; } // The buff ID associated with the minion(s)
			public List<ProjModel> ProjData { get; set; } // The projectile ID(s) and additional data of the minion(s) (Usually contains only 1 element)

			// Helper method to represent the item this minion model is associated with
			public override string ToString() => ItemID < Terraria.ID.ItemID.Count ? Lang.GetItemNameValue(ItemID) : ItemLoader.GetItem(ItemID).DisplayName.ToString();

			// Helper method to convert from dictionary to class
			public static MinionModel FromDictionary(Dictionary<string, object> dict) {
				var model = new MinionModel {
					ItemID = dict.ContainsKey("ItemID") ? Convert.ToInt32(dict["ItemID"]) : 0,
					BuffID = dict.ContainsKey("BuffID") ? Convert.ToInt32(dict["BuffID"]) : 0
				};

				if (dict.ContainsKey("ProjData")) {
					var projDataDict = dict["ProjData"] as List<Dictionary<string, object>>;
					model.ProjData = projDataDict.Select(pDict =>
						new ProjModel(
							pDict.ContainsKey("ProjID") ? Convert.ToInt32(pDict["ProjID"]) : 0,
							pDict.ContainsKey("Slot") ? Convert.ToSingle(pDict["Slot"]) : 1f)
						).ToList();
				}
				else {
					model.ProjData = new List<ProjModel>();
				}

				return model;
			}
		}

		public static List<MinionModel> supportedMinions = new List<MinionModel>();

		public static bool IntegrationSuccessful { get; private set; }

		public override void PostAddRecipes() {
			// For best results, this code is in PostAddRecipes
			supportedMinions.Clear();
			IntegrationSuccessful = false;

			if (ModLoader.TryGetMod("SummonersAssociation", out var summonersAssociation) && summonersAssociation.Version >= SummonersAssociationAPIVersion) {
				object getSupportedMinionsResponse = summonersAssociation.Call("GetSupportedMinions", Mod, SummonersAssociationAPIVersion.ToString());
				if (getSupportedMinionsResponse is List<Dictionary<string, object>> supportedMinionsList) {
					supportedMinions = supportedMinionsList.Select(dict => MinionModel.FromDictionary(dict)).ToList();

					IntegrationSuccessful = true;
				}
			}
		}

		public override void OnModUnload() {
			supportedMinions.Clear();
			IntegrationSuccessful = false;
		}

		// This method shows an example of using the supportedMinions data for something cool in your mod.
		public static int GetActiveMinionBuffs(Player player) {
			if (supportedMinions.Count == 0) // supportedMinions might be empty, if SummonersAssociation isn't present or something goes wrong.
				return 0;

			int count = 0;
			foreach (var model in supportedMinions) {
				if (model.BuffID > 0 && player.HasBuff(model.BuffID))
					count++;
			}

			return count;
		}

		// This utility method shows how you can easily retreive a list of projectile IDs that are associated with a given buff type.
		// For example, this will return two IDs if you provide BuffID.TwinEyesMinion
		public static List<int> GetProjectileIDsAssociatedWithBuff(int buffType) {
			List<int> projectileIDs = new List<int>();
			foreach (var model in supportedMinions) {
				if (model.ItemID > 0 && model.BuffID == buffType) {
					projectileIDs = model.ProjData.Select(d => d.ProjID).ToList();
					break;
				}
			}
			return projectileIDs;
		}

		// This utility method shows how you can easily retreive a list of projectile IDs that are associated with a given item type.
		// For example, this will return two IDs if you provide ItemID.OpticStaff
		public static List<int> GetProjectileIDsAssociatedWithItem(int itemType) {
			List<int> projectileIDs = new List<int>();
			foreach (var model in supportedMinions) {
				if (model.BuffID > 0 && model.ItemID == itemType) {
					projectileIDs = model.ProjData.Select(d => d.ProjID).ToList();
					break;
				}
			}
			return projectileIDs;
		}
	}
}
*/
