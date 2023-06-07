using SummonersAssociation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	//Handles registering MinionModels (both automatically and by mod calls)
	public class SummonersAssociationSystem : ModSystem
	{
		internal static bool SupportedMinionsFinalized { get; private set; }

		internal static int MagicMirrorRecipeGroup { get; private set; }

		internal static LocalizedText RecipeGroupGenericText { get; private set; }

		public override void OnModLoad() {
			SupportedMinionsFinalized = false;

			RecipeGroupGenericText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"RecipeGroups.RecipeGroupGeneric"));
		}

		public override void AddRecipeGroups() {
			// Automatically register MinionModels
			var item = new Item();
			var projectile = new Projectile();
			for (int i = ItemID.Count; i < ItemLoader.ItemCount; i++) {
				item = ItemLoader.GetItem(i).Item;
				if (item.buffType > 0 && item.shoot >= ProjectileID.Count) {
					projectile = ProjectileLoader.GetProjectile(item.shoot).Projectile;
					if (projectile.minionSlots > 0) {
						// Avoid automatic support for manually supported
						if (!SummonersAssociation.SupportedMinions.Any(x => x.ItemID == i || x.ContainsProjID(projectile.type) || x.BuffID == item.buffType)) {
							AddMinion(new MinionModel(item.type, item.buffType, projectile.type));
						}
					}
				}
			}

			SupportedMinionsFinalized = true;

			var group = new RecipeGroup(() => RecipeGroupGenericText.Format(Language.GetTextValue("LegacyMisc.37") , Lang.GetItemNameValue(ItemID.MagicMirror)), new int[]
			{
				ItemID.MagicMirror,
				ItemID.IceMirror
			});
			MagicMirrorRecipeGroup = RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
		}

		//Examples:
		//############
		//if (ModLoader.TryGetMod("SummonersAssociation", out Mod summonersAssociation)) 
		//{
		//  Calls here
		//}
		//
		//	Regular call for a regular summon weapon
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		ProjectileType<MinionProjectile>()
		//	);
		//
		//  If the weapon summons two (or more) minions
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new List<int> {
		//		ProjectileType<MinionProjectile1>(),
		//		ProjectileType<MinionProjectile2>()
		//		}
		//	);
		//
		//  If you need to add any additional info to the projectile, such as overriding the minion slot
		//  (for example useful if you have a Stardust Dragon-like minion and you only want it to count one segment towards the number of summoned minions)
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new Dictionary<string, object>() {
		//			["ProjID"] = ProjectileType<MinionProjectile>(),
		//			["Slot"] = 1f
		//		}
		//	);
		//
		//  If you need to add any additional info to multiple projectiles, such as overriding the minion slot
		//  (for example useful if you have some complex minion that consists of multiple parts)
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new List<Dictionary<string, object>> {
		//			new Dictionary<string, object>() {
		//				["ProjID"] = ProjectileType<MinionProjectile1>(),
		//				["Slot"] = 0.25f
		//			},
		//			new Dictionary<string, object>() {
		//				["ProjID"] =  ProjectileType<MinionProjectile2>(),
		//				["Slot"] = 1f //This can be omitted aswell (then it'll default to Projectile.minionSlots), only ProjID is mandatory
		//			}
		//		}
		//	);
		//
		//	Storm Tiger-like "counter" projectile that is a minion but should not be teleported
		//  Or if a minion hovers over your head and should never move
		//	summonersAssociation.Call(
		//		"AddTeleportConditionMinion",
		//		 ProjectileType<MinionCounterProjectile>()
		//	);
		//	
		//  Customizable condition (no condition: defaults to false):
		//	summonersAssociation.Call(
		//		"AddTeleportConditionMinion",
		//	    ProjectileType<MinionProjectile>(),
		//	    (Func<Projectile, bool>) ((Projectile p) => false) //return false here to prevent it from teleporting, otherwise, true
		//	);
		//
		//  Get a copy of the stored information about all minions (See SummonersAssociationIntegrationExample.cs for more info)
		//  var data = (List<Dictionary<string, object>>)summonersAssociation.Call(
		//		"GetSupportedMinions",
		//	);
		//

		public static object Call(params object[] args) {
			/* message string, then 
			 * if "AddMinionInfo": all calls have the same number of args
			 * int, int, List<Dictionary<string, object>>
			 * or
			 * int, int, List<int>
			 * or
			 * int, int, Dictionary<string, object>
			 * or
			 * int, int, int
			 * 
			 * else if "TeleportConditionMinions":
			 * int
			 * or
			 * int, Func<Projectile, bool>
			 * 
			 * else if "GetSupportedMinions":
			 * Mod, apiVersionString
			 * returns List<Dictionary<string, object>>
			 * ...
			 */
			var modSA = SummonersAssociation.Instance;
			var logger = modSA.Logger;
			try {
				string message = args[0] as string;
				if (message == "AddMinionInfo") {
					if (SupportedMinionsFinalized)
						throw new Exception($"{modSA.Name} Call Error: The attempted message, \"{message}\", was sent too late. {modSA.Name} expects Call messages to happen during Mod.PostSetupContent.");

					int itemID = Convert.ToInt32(args[1]);
					int buffID = Convert.ToInt32(args[2]);

					if (itemID <= 0 || itemID >= ItemLoader.ItemCount)
						throw new Exception("Invalid item '" + itemID + "' registered");

					string itemMsg = " ### Minion from item '" + (itemID < ItemID.Count ? Lang.GetItemNameValue(itemID) : ItemLoader.GetItem(itemID).DisplayName) + "' not added";

					if (buffID <= 0 || buffID >= BuffLoader.BuffCount)
						throw new Exception("Invalid buff '" + buffID + "' registered" + itemMsg);

					object projArg = args[3];
					if (projArg is List<Dictionary<string, object>> projDataDicts) {
						if (projDataDicts.Count == 0) throw new Exception("ProjModel list empty" + itemMsg);

						var addedProjIDs = new HashSet<int>(); // Sanitize lists to not contain duplicates
						var projDataList = new List<ProjModel>();
						foreach (var projModelDict in projDataDicts) {
							var projModel = ProjModel.FromDictionary(projModelDict, itemMsg);

							int projID = projModel.ProjID;
							if (!addedProjIDs.Contains(projID))
							{
								projDataList.Add(projModel);
								addedProjIDs.Add(projID);
							}
						}

						AddMinion(new MinionModel(itemID, buffID, projDataList));
					}
					else if (projArg is List<int> projIDs) {
						if (projIDs.Count == 0) throw new Exception("Projectile list empty" + itemMsg);

						// Sanitize list via Distinct() to not contain duplicates
						foreach (int projID in projIDs.Distinct()) {
							CheckProj(itemMsg, projID);
						}

						AddMinion(new MinionModel(itemID, buffID, projIDs));
					}
					else if (projArg is Dictionary<string, object> projModelDict) {
						AddMinion(new MinionModel(itemID, buffID, ProjModel.FromDictionary(projModelDict, itemMsg)));
					}
					else if (projArg is int) {
						int projID = Convert.ToInt32(args[3]);
						CheckProj(itemMsg, projID);
						AddMinion(new MinionModel(itemID, buffID, projID));
					}
					else {
						//TODO should this case exist? throw exception perhaps?
						return "Failure";
					}
					return "Success";
				}
				else if (message == "AddTeleportConditionMinion") {
					//New with v0.4.6
					int projID = Convert.ToInt32(args[1]);
					if (projID <= 0 || projID >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile '" + projID + "' registered");

					Func<Projectile, bool> func = SummonersAssociation.ProjectileFalse;
					if (args.Length == 3 && args[2] is Func<Projectile, bool>) {
						func = args[2] as Func<Projectile, bool>;
					}

					SummonersAssociation.TeleportConditionMinions[projID] = func;
					return "Success";
				}
				else if (message == "GetSupportedMinions") {
					//New with v0.4.7
					var mod = args[1] as Mod;
					if (mod == null) {
						throw new Exception($"Call Error: The Mod argument for the attempted message, \"{message}\" has returned null.");
					}
					var apiVersion = args[2] is string ? new Version(args[2] as string) : modSA.Version; // Future-proofing. Allowing new info to be returned while maintaining backwards compat if necessary.

					logger.Info($"{(mod.DisplayName ?? "A mod")} has registered for {message} via Call");

					if (!SupportedMinionsFinalized) {
						logger.Warn($"Call Warning: The attempted message, \"{message}\", was sent too early. Expect the Call message to return incomplete data. For best results, call in PostAddRecipes.");
					}

					var list = SummonersAssociation.SupportedMinions.Select(m => m.ConvertToDictionary(apiVersion)).ToList();
					return list;
				}
			}
			catch (Exception e) {
				logger.Error(modSA.Name + " Call Error: " + e.StackTrace + e.Message);
			}
			return "Failure";
		}

		private static void CheckProj(string itemMsg, int type) {
			if (type <= 0 || type >= ProjectileLoader.ProjectileCount)
				throw new Exception("Invalid projectile '" + type + "' registered" + itemMsg);
		}

		/// <summary>
		/// Almost the same as Add, but merges projectile lists on the same buff and registers its projectile(s) without creating a new model
		/// </summary>
		private static void AddMinion(MinionModel model) {
			MinionModel existing = SummonersAssociation.SupportedMinions.SingleOrDefault(m => m.BuffID == model.BuffID);
			if (existing != null) {
				foreach (var data in model.ProjData) {
					if (!existing.ContainsProjID(data.ProjID)) {
						existing.ProjData.Add(data);
					}
				}
			}
			else {
				SummonersAssociation.SupportedMinions.Add(model);
			}
		}
	}
}
