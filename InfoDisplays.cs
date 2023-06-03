using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SummonersAssociation
{
	public class MinionInfoDisplay : InfoDisplay
	{
		public override bool Active() => Config.Instance.InfoIcon;

		public override string DisplayValue(ref Color displayColor) {
			Player player = Main.LocalPlayer;
			double minionCount = Math.Round(player.slotsMinions, 2);
			if (minionCount <= 0) {
				displayColor = InactiveInfoTextColor;
			}

			return UISystem.MinionSlotsIconText.Format(Math.Round(player.slotsMinions, 2), player.maxMinions);
		}
	}

	public class SentryInfoDisplay : InfoDisplay
	{
		public override bool Active() => Config.Instance.InfoIcon;

		public override string DisplayValue(ref Color displayColor) {
			Player player = Main.LocalPlayer;
			UISystem.GetSentryNameToCount(out int sentryCount, onlyCount: true);
			if (sentryCount <= 0) {
				displayColor = InactiveInfoTextColor;
			}

			return UISystem.SentrySlotsIconText.Format(sentryCount, player.maxTurrets);
		}
	}
}
