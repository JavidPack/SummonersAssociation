using Newtonsoft.Json;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SummonersAssociation
{
	public class ServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();

		/// <summary>
		/// If true, disables the hooks required for the feature to work, makes the right click a regular "targeting" feature, and if a reticle
		/// manages to spawn (cheatsheet), it's automatically despawned
		/// </summary>
		[ReloadRequired]
		[DefaultValue(false)]
		public bool DisableAdvancedTargetingFeature;

		[DefaultValue(false)]
		public bool PersistentReticle;

		[Header("HintToClientConfig")]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public bool Hint => true;

		public static bool IsPlayerLocalServerOwner(int whoAmI) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			return NetMessage.DoesPlayerSlotCountAsAHost(whoAmI);
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI)) {
				message = SummonersAssociation.AcceptClientChangesText.ToString();
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
