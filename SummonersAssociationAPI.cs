using SummonersAssociation.Models;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace SummonersAssociation
{
	/// <summary>
	/// Class that provides access to the data of all supported minions by this mod. Use <see cref="GetSupportedMinions"/> to get the raw data and operate on it how you want.
	/// Or use the helper methods to circumvent working with the MinionModel class (not the full scope though).
	/// </summary>
	public static class SummonersAssociationAPI
	{
		/// <summary>
		/// Returns an IEnumerable[MinionModel] that is a copy of this mod's data. Cache the result.
		/// </summary>
		/// <returns>Data of all supported minions by this mod</returns>
		public static IEnumerable<MinionModel> GetSupportedMinions() =>
			SummonersAssociation.SupportedMinions.Select(model => new MinionModel(model));

		/// <summary>
		/// Returns the number of "minion" buffs currently active on the player.
		/// </summary>
		/// <param name="player">The player</param>
		/// <returns>Number of "minion" buffs</returns>
		public static int GetActiveMinionBuffs(Player player) {
			int count = 0;
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.BuffID > 0 && player.HasBuff(model.BuffID))
					count++;
			}

			return count;
		}

		/// <summary>
		/// Get a list of projectile IDs that are associated with a given buff type.
		/// For example, this will return two IDs if you provide BuffID.TwinEyesMinion
		/// </summary>
		/// <param name="buffType">Buff type</param>
		/// <returns>List of projectile IDs. Empty list if buff is not associated with a minion</returns>
		public static List<int> GetProjectileIDsAssociatedWithBuff(int buffType) {
			List<int> projectileIDs = new List<int>();
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.ItemID > 0 && model.BuffID == buffType) {
					projectileIDs = model.ProjectileIDs;
					break;
				}
			}
			return projectileIDs;
		}

		/// <summary>
		/// Get a list of projectile IDs that are associated with a given item type.
		/// For example, this will return two IDs if you provide ItemID.OpticStaff
		/// </summary>
		/// <param name="itemType">Item type</param>
		/// <returns>List of projectile IDs. Empty list if item is not associated with a minion</returns>
		public static List<int> GetProjectileIDsAssociatedWithItem(int itemType) {
			List<int> projectileIDs = new List<int>();
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.BuffID > 0 && model.ItemID == itemType) {
					projectileIDs = model.ProjectileIDs;
					break;
				}
			}
			return projectileIDs;
		}

		/// <summary>
		/// Get the item ID that is associated with a given buff type.
		/// For example, this will return the value of ItemID.OpticStaff if you provide BuffID.TwinEyesMinion
		/// </summary>
		/// <param name="buffType">Buff type</param>
		/// <returns>Item ID. 0 if buff is not associated with a minion</returns>
		public static int GetItemIDAssociatedWithBuff(int buffType) {
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.ItemID > 0 && model.BuffID == buffType) {
					return model.ItemID;
				}
			}
			return 0;
		}

		/// <summary>
		/// Get the buff ID that is associated with a given item type.
		/// For example, this will return the value of BuffID.TwinEyesMinion if you provide ItemID.OpticStaff
		/// </summary>
		/// <param name="itemType">Item type</param>
		/// <returns>Buff ID. 0 if item is not associated with a minion</returns>
		public static int GetBuffIDAssociatedWithItem(int itemType) {
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.BuffID > 0 && model.ItemID == itemType) {
					return model.BuffID;
				}
			}
			return 0;
		}

		/// <summary>
		/// Get the buff ID that is associated with a given projectile type.
		/// For example, this will return the value of BuffID.TwinEyesMinion if you provide ProjectileID.Spazmamini
		/// </summary>
		/// <param name="projType">Projectile type</param>
		/// <returns>Buff ID. 0 if projectile is not associated with a minion</returns>
		public static int GetBuffIDAssociatedWithProjectile(int projType) {
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.BuffID > 0 && model.ProjectileIDs.Contains(projType)) {
					return model.BuffID;
				}
			}
			return 0;
		}

		/// <summary>
		/// Get the item ID that is associated with a given projectile type.
		/// For example, this will return the value of ItemID.OpticStaff if you provide ProjectileID.Spazmamini
		/// </summary>
		/// <param name="projType">Projectile type</param>
		/// <returns>Item ID. 0 if projectile is not associated with a minion</returns>
		public static int GetItemIDAssociatedWithProjectile(int projType) {
			foreach (var model in SummonersAssociation.SupportedMinions) {
				if (model.ItemID > 0 && model.ProjectileIDs.Contains(projType)) {
					return model.ItemID;
				}
			}
			return 0;
		}
	}
}
