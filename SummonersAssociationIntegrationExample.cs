/*using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace <YourModNameHere>
{
	// This class provides an example of advanced Summoners Association integration utilizing the "GetSupportedMinions" Mod.Call that other Mods can copy into their mod's source code.
	// By copying this class into your mod, you can access Summoners Association minion data reliably and with type safety without requiring a strong dependency.
	public static class SummonersAssociationIntegration
	{
		// Summoners Association might add new features, so a version is passed into GetSupportedMinions. 
		// If a new version of the GetSupportedMinions Call is implemented, find this class in the Summoners Association Github once again and replace this version with the new version: https://github.com/JavidPack/SummonersAssociation/blob/master/SummonersAssociationIntegrationExample.cs
		private static readonly Version SummonersAssociationAPIVersion = new Version(0, 4, 7); // Do not change this yourself.

		public class MinionModel
		{
			public int ItemID { get; set; } //The item ID associated with the minion(s)
			public int BuffID { get; set; } //The buff ID associated with the minion(s)
			public List<int> ProjectileIDs { get; set; } //The projectile ID(s) of the minion(s) (Usually contains only 1 element)
			public List<float> Slots { get; set; } //The minionSlots of the minion(s) (matching the index of ProjectileIDs)

			//Helper method to represent the item this minion model is associated with
			public override string ToString() => Terraria.ID.ItemID.GetUniqueKey(ItemID);
		}

		public static List<MinionModel> supportedMinions = new List<MinionModel>();

		public static bool DoSummonersAssociationIntegration(Mod mod) {
			// Make sure to call this method in PostAddRecipes or later for best results: SummonersAssociationIntegration.DoSummonersAssociationIntegration(this);
			supportedMinions.Clear();
			Mod SummonersAssociation = ModLoader.GetMod("SummonersAssociation");
			if (SummonersAssociation != null && SummonersAssociation.Version >= SummonersAssociationAPIVersion) {
				object currentSupportedMinionsResponse = SummonersAssociation.Call("GetSupportedMinions", mod, SummonersAssociationAPIVersion.ToString());
				if (currentSupportedMinionsResponse is List<Dictionary<string, object>> supportedMinionsList) {
					supportedMinions = supportedMinionsList.Select(dict => new MinionModel() {
						ItemID = dict.ContainsKey("ItemID") ? Convert.ToInt32(dict["ItemID"]) : 0,
						BuffID = dict.ContainsKey("BuffID") ? Convert.ToInt32(dict["BuffID"]) : 0,
						ProjectileIDs = dict.ContainsKey("ProjectileIDs") ? dict["ProjectileIDs"] as List<int> : new List<int>(),
						Slots = dict.ContainsKey("Slots") ? dict["Slots"] as List<float> : new List<float>(),
					}).ToList();
					return true;
				}
			}
			return false;
		}

		public static void UnloadSummonersAssociationIntegration() {
			// Make sure to call this method in your Mod.Unload method to properly release memory: SummonersAssociationIntegration.UnloadSummonersAssociationIntegration();
			supportedMinions.Clear();
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
					projectileIDs = model.ProjectileIDs;
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
					projectileIDs = model.ProjectileIDs;
					break;
				}
			}
			return projectileIDs;
		}
	}
}
*/