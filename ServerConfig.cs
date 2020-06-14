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

		[Header("Hint: To go to the client config containing UI adjustments, press the '<' arrow in the bottom left")]
		[Label("Hint")]
		[JsonIgnore]
		public bool Hint => true;

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message) {
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			message = "You are not the server owner so you can not change this config";
			return false;
		}
	}
}
