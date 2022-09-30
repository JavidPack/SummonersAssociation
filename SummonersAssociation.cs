using SummonersAssociation.Items;
using SummonersAssociation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	public class SummonersAssociation : Mod
	{
		internal static List<MinionModel> SupportedMinions;
		/// <summary>
		/// Hardcoded special vanilla minions that summon a non-1f amount of minions on use
		/// </summary>
		internal static Dictionary<int, float> SlotsFilledPerUse;

		/// <summary>
		/// Mainly for "Counter" projectiles that count as minions but should not be teleported (by the Minion Control Rod)
		/// </summary>
		internal static Dictionary<int, Func<Projectile, bool>> TeleportConditionMinions;

		internal bool SupportedMinionsFinalized = false;

		public static SummonersAssociation Instance { get; private set; }
		
		internal static bool ProjectileFalse(Projectile p) => false;

		/// <summary>
		/// Array of the different minion book types. Simple is 0, Normal is 1, Auto is 2
		/// </summary>
		public static int[] BookTypes;

		public override void Load() {
			Instance = this;

			SupportedMinions = new List<MinionModel>() {
				new MinionModel(ItemID.BabyBirdStaff, BuffID.BabyBird, ProjectileID.BabyBird),
				new MinionModel(ItemID.AbigailsFlower, BuffID.AbigailMinion, ProjectileID.AbigailCounter),
				new MinionModel(ItemID.SlimeStaff, BuffID.BabySlime, ProjectileID.BabySlime),
				new MinionModel(ItemID.FlinxStaff, BuffID.FlinxMinion, ProjectileID.FlinxMinion),
				new MinionModel(ItemID.VampireFrogStaff, BuffID.VampireFrog, ProjectileID.VampireFrog),
				new MinionModel(ItemID.HornetStaff, BuffID.HornetMinion, ProjectileID.Hornet),
				new MinionModel(ItemID.ImpStaff, BuffID.ImpMinion, ProjectileID.FlyingImp),
				new MinionModel(ItemID.SpiderStaff, BuffID.SpiderMinion, new List<int>() { ProjectileID.VenomSpider, ProjectileID.JumperSpider, ProjectileID.DangerousSpider }),
				new MinionModel(ItemID.SanguineStaff, BuffID.BatOfLight, ProjectileID.BatOfLight),
				new MinionModel(ItemID.OpticStaff, BuffID.TwinEyesMinion, new List<int>() { ProjectileID.Retanimini, ProjectileID.Spazmamini }),
				new MinionModel(ItemID.PirateStaff, BuffID.PirateMinion, new List<int>() { ProjectileID.OneEyedPirate, ProjectileID.SoulscourgePirate, ProjectileID.PirateCaptain }),
				new MinionModel(ItemID.Smolstar, BuffID.Smolstar, ProjectileID.Smolstar),
				new MinionModel(ItemID.PygmyStaff, BuffID.Pygmies, new List<int>() { ProjectileID.Pygmy, ProjectileID.Pygmy2, ProjectileID.Pygmy3, ProjectileID.Pygmy4 }),
				new MinionModel(ItemID.StormTigerStaff, BuffID.StormTiger, ProjectileID.StormTigerGem),
				new MinionModel(ItemID.XenoStaff, BuffID.UFOMinion, ProjectileID.UFOMinion),
				new MinionModel(ItemID.RavenStaff, BuffID.Ravens, ProjectileID.Raven),
				new MinionModel(ItemID.TempestStaff, BuffID.SharknadoMinion, ProjectileID.Tempest),
				new MinionModel(ItemID.DeadlySphereStaff, BuffID.DeadlySphere, ProjectileID.DeadlySphere),
				new MinionModel(ItemID.StardustDragonStaff, BuffID.StardustDragonMinion, ProjectileID.StardustDragon2, 1f),
				new MinionModel(ItemID.StardustCellStaff, BuffID.StardustMinion, ProjectileID.StardustCellMinion),
				new MinionModel(ItemID.EmpressBlade, BuffID.EmpressBlade, ProjectileID.EmpressBlade)
			};

			//For SlotsFilledPerUse we can't use MinionModel.GetSlotsPerProjectile because thats just a list of projectiles, and not those that are summoned once on use
			SlotsFilledPerUse = new Dictionary<int, float> {
				//[ItemID.SpiderStaff] = 0.75f //Changed to 1 in 1.4
			};

			TeleportConditionMinions = new Dictionary<int, Func<Projectile, bool>>() {
				[ProjectileID.StormTigerGem] = ProjectileFalse,
				[ProjectileID.AbigailCounter] = ProjectileFalse
			};
		}

		public override void PostSetupContent()
			//don't change order here (simple is first, normal is second, automatic is third)
			=> BookTypes = new int[] {
				ModContent.ItemType<MinionHistoryBookSimple>(),
				ModContent.ItemType<MinionHistoryBook>(),
				ModContent.ItemType<MinionHistoryBookAuto>()
			};

		public override void Unload() {
			SupportedMinions = null;
			SlotsFilledPerUse = null;
			TeleportConditionMinions = null;
			BookTypes = null;

			Instance = null;
		}

		public override object Call(params object[] args) {
			return SummonersAssociationSystem.Call(args);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();

			if (Enum.IsDefined(typeof(PacketType), type)) {
				var packetType = (PacketType)type;
				switch (packetType) {
					case PacketType.SpawnTarget:
						MinionControlRod.HandleSpawnTarget(reader);
						break;
					case PacketType.ConfirmTargetToClient:
						MinionControlRod.HandleConfirmTargetToClient(reader);
						break;
					default:
						Logger.Warn("'None' packet type received");
						break;
				}
			}
			else {
				Logger.Warn("Undefined packet type received: " + type);
			}
		}
	}
}
