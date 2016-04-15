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

		public override void AddCraftGroups()
		{
			AddCraftGroup("MinionStaffs", Lang.misc[37] + " " + "Minion Staff", ItemID.SlimeStaff, 
				ItemID.ImpStaff, ItemID.HornetStaff, ItemID.SpiderStaff, ItemID.OpticStaff, 
				ItemID.PirateStaff, ItemID.PygmyStaff, ItemID.XenoStaff, ItemID.RavenStaff,
				ItemID.TempestStaff, ItemID.DeadlySphereStaff, ItemID.StardustCellStaff, ItemID.StardustDragonStaff);
			AddCraftGroup("MagicMirrors", Lang.misc[37] + " " + "Magic Mirror", ItemID.IceMirror,ItemID.MagicMirror);
		}

		// TODO: summonersAssociation.Call("Buff->Projectile", BuffType("CoolMinionBuff"), ProjectileType("CoolMinionProjectile")); style call.
		//public override object Call(params object[] args)
		//{
		//	return base.Call(args);
		//}
	}
}
