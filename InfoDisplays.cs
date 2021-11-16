using System;
using Terraria;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	public class MinionInfoDisplay : InfoDisplay
	{
		public override void SetStaticDefaults() {
			InfoName.SetDefault("Minion Slots");
		}

		public override bool Active() {
			return Config.Instance.InfoIcon;
		}

		public override string DisplayValue() {
			Player player = Main.LocalPlayer;
			return "" + Math.Round(player.slotsMinions, 2) + " / " + player.maxMinions + " Minion Slots";
		}
	}

	public class SentryInfoDisplay : InfoDisplay
	{
		public override void SetStaticDefaults() {
			InfoName.SetDefault("Sentry Slots");
		}

		public override bool Active() {
			return Config.Instance.InfoIcon;
		}

		public override string DisplayValue() {
			Player player = Main.LocalPlayer;
			UISystem.GetSentryNameToCount(out int sentryCount, onlyCount: true);
			return "" + sentryCount + " / " + player.maxTurrets + " Sentry Slots";
		}
	}
}
