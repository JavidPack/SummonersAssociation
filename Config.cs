using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SummonersAssociation
{
	[Label("Client Config")]
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static Config Instance => ModContent.GetInstance<Config>();

		//0 - 1 : 0 - Main.screenWidth
		//0 - 1 : 0 - Main.screenHeight
		public const float DefaultX = 0.89f;
		public const float DefaultY = 0.87f;

		[DefaultValue(typeof(Vector2), "0.89, 0.87")]
		[Label("Max Minion/Sentry Icon Offset")]
		public Vector2 Offset;

		[Header("Hint: To go to the server config containing feature toggles, press the '>' arrow in the bottom right")]
		[Label("Hint")]
		[JsonIgnore]
		public bool Hint => true;
	}
}
