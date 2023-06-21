using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace SummonersAssociation
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static Config Instance => ModContent.GetInstance<Config>();

		[DefaultValue(true)]
		public bool InventoryIcon;

		[DefaultValue(true)]
		public bool InfoIcon;

		//0 - 1 : 0 - Main.screenWidth
		//0 - 1 : 0 - Main.screenHeight
		public const float DefaultX = 0.89f;
		public const float DefaultY = 0.87f;

		[Header("Blank")]
		[DefaultValue(typeof(Vector2), "0.89, 0.87")]
		public Vector2 Offset;

		[Header("HintToServerConfig")]
		[JsonIgnore]
		[ShowDespiteJsonIgnore]
		public bool Hint => true;

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context) {
			//Correct invalid values
			Offset.X = Utils.Clamp(Offset.X, 0f, 1f);
			Offset.Y = Utils.Clamp(Offset.Y, 0f, 1f);
		}
	}
}
