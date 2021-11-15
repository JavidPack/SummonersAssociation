using Newtonsoft.Json;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SummonersAssociation
{
	[Label("Server Config")]
	public class ServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static ServerConfig Instance => ModContent.GetInstance<ServerConfig>();

		/// <summary>
		/// If true, disables the hooks required for the feature to work, makes the right click a regular "targeting" feature, and if a reticle
		/// manages to spawn (cheatsheet), it's automatically despawned
		/// </summary>
		[Label("Disable Advanced Targeting Feature")]
		[Tooltip("If toggled, disables the targeting reticle feature from the Minion Control Rod")]
		[ReloadRequired]
		[DefaultValue(false)]
		public bool DisableAdvancedTargetingFeature;

		[Label("Enable Persistent Reticle")]
		[Tooltip("If toggled, makes the reticle from the Minion Control Rod never disappear when going out of range")]
		[DefaultValue(false)]
		public bool PersistentReticle;

		[Header("Hint: To go to the client config containing UI adjustments, press the '<' arrow in the bottom left")]
		[Label("Hint")]
		[JsonIgnore]
		public bool Hint => true;

		public static bool IsPlayerLocalServerOwner(int whoAmI) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			for (int i = 0; i < Main.maxPlayers; i++) {
				RemoteClient client = Netplay.Clients[i];
				if (client.State == 10 && i == whoAmI && client.Socket.GetRemoteAddress().IsLocalHost()) {
					return true;
				}
			}
			return false;
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI)) {
				message = "You are not the server owner so you can not change this config";
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
