using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	public class SummonersAssociation : Mod
	{
		public SummonersAssociation()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup group = new RecipeGroup(() => Lang.misc[37] + " Minion Staff", new int[]
			{
				ItemID.SlimeStaff,
				ItemID.ImpStaff,
				ItemID.HornetStaff,
				ItemID.SpiderStaff,
				ItemID.OpticStaff,
				ItemID.PirateStaff,
				ItemID.PygmyStaff,
				ItemID.XenoStaff,
				ItemID.RavenStaff,
				ItemID.TempestStaff,
				ItemID.DeadlySphereStaff,
				ItemID.StardustCellStaff,
				ItemID.StardustDragonStaff
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MinionStaffs", group);

			group = new RecipeGroup(() => Lang.misc[37] + " Magic Mirror", new int[]
			{
				ItemID.IceMirror,
				ItemID.MagicMirror
			});
			RecipeGroup.RegisterGroup("SummonersAssociation:MagicMirrors", group);
		}

		// TODO: summonersAssociation.Call("Buff->Projectile", BuffType("CoolMinionBuff"), ProjectileType("CoolMinionProjectile")); style call.
		//public override object Call(params object[] args)
		//{
		//	return base.Call(args);
		//}
	}
}
