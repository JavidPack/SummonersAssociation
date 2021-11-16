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

		[Label("Inventory Icon Toggle")]
		[Tooltip("Toggle the minion/sentry inventory slot elements")]
		[DefaultValue(true)]
		public bool InventoryIcon;

		[Label("Info Icon Toggle")]
		[Tooltip("Toggle the minion/sentry info display entries")]
		[DefaultValue(true)]
		public bool InfoIcon;

		//0 - 1 : 0 - Main.screenWidth
		//0 - 1 : 0 - Main.screenHeight
		public const float DefaultX = 0.89f;
		public const float DefaultY = 0.87f;

		[Header(" ")]
		[DefaultValue(typeof(Vector2), "0.89, 0.87")]
		[Label("Inventory Icon Offset")]
		[Tooltip("Change the position of the 'Inventory Icons' relative to the screen")]
		public Vector2 Offset;

		[Header("Hint: To go to the server config containing feature toggles, press the '>' arrow in the bottom right")]
		[Label("Hint")]
		[JsonIgnore]
		public bool Hint => true;
	}
}
