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
						if (!SummonersAssociation.SupportedMinions.Any(x => x.ItemID == i || x.ProjectileIDs.Contains(projectile.type) || x.BuffID == item.buffType)) {
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
		//  If the weapon summons two minions
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
		//  If you want to override the minionSlots of the projectile
		//  (for example useful if you have a Stardust Dragon-like minion and you only want it to count one segment towards the number of summoned minions)
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		ProjectileType<MinionProjectile>(),
		//		1f
		//	);
		//
		//  If you want to override the minionSlots of the projectiles you specify (same order)
		//  (for example useful if you have some complex minion that consists of multiple parts)
		//	summonersAssociation.Call(
		//		"AddMinionInfo",
		//		ItemType<MinionItem>(),
		//		BuffType<MinionBuff>(),
		//		new List<int> {
		//		ProjectileType<MinionProjectile1>(),
		//		ProjectileType<MinionProjectile2>()
		//		},
		//		new List<float> {
		//		0.25f,
		//		0.5f
		//		}
		//	);
		//
		//	Storm Tiger-like "counter" projectile that is a minion but should not be teleported
		//  Or if a minion hovers over your head and should never move
		//	summonersAssociation.Call(
		//		"AddTeleportConditionMinion",
		//		ProjectileType<MinionCounterProjectile>()
		//	);
		//	
		//  Customizable condition (no condition: defaults to false):
		//	summonersAssociation.Call(
		//		"AddTeleportConditionMinion",
		//	    ModContent.ProjectileType<MinionProjectile>(),
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
			 * if "AddMinionInfo":
			 * int, int, List<int>, List<float>
			 * or
			 * int, int, List<int>
			 * or
			 * int, int, int, float
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

					if (itemID <= 0 || itemID >= ItemLoader.ItemCount) throw new Exception("Invalid item '" + itemID + "' registered");

					string itemMsg = " ### Minion from item '" + Lang.GetItemNameValue(itemID) + "' not added";

					if (buffID <= 0 || buffID >= BuffLoader.BuffCount) throw new Exception("Invalid buff '" + buffID + "' registered" + itemMsg);

					if (args[3] is List<int>) {
						var projIDs = args[3] as List<int>;
						if (projIDs.Count == 0) throw new Exception("Projectile list empty" + itemMsg);

						foreach (int type in projIDs) {
							if (type <= 0 || type >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile '" + type + "' registered" + itemMsg);
						}

						if (args.Length == 5) {
							var slots = args[4] as List<float>;
							if (projIDs.Count != slots.Count)
								throw new Exception("Length of the projectile list does not match up with the length of the slot list" + itemMsg);

							AddMinion(new MinionModel(itemID, buffID, projIDs, slots));
						}
						else {
							AddMinion(new MinionModel(itemID, buffID, projIDs));
						}
					}
					else {
						int projID = Convert.ToInt32(args[3]);
						if (projID <= 0 || projID >= ProjectileLoader.ProjectileCount) throw new Exception("Invalid projectile '" + projID + "' registered" + itemMsg);
						
						if (args.Length == 5) {
							float slot = Convert.ToSingle(args[4]);

							AddMinion(new MinionModel(itemID, buffID, projID, slot));
						}
						else {
							AddMinion(new MinionModel(itemID, buffID, projID));
						}
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

		/// <summary>
		/// Almost the same as Add, but merges projectile lists on the same buff and registers its item without creating a new model
		/// </summary>
		private static void AddMinion(MinionModel model) {
			MinionModel existing = SummonersAssociation.SupportedMinions.SingleOrDefault(m => m.BuffID == model.BuffID);
			if (existing != null) {
				for (int i = 0; i < model.ProjectileIDs.Count; i++) {
					int id = model.ProjectileIDs[i];
					if (!existing.ProjectileIDs.Contains(id)) {
						existing.ProjectileIDs.Add(id);
						existing.Slots.Add(model.Slots[i]);
					}
				}
			}
			else {
				SummonersAssociation.SupportedMinions.Add(model);
			}
		}
	}
}
