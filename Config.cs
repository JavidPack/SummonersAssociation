using Microsoft.Xna.Framework;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SummonersAssociation
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
		public static Config Instance;

		//0 - 1 : 0 - Main.screenWidth
		//0 - 1 : 0 - Main.screenHeight
		public const float DefaultX = 0.89f;
		public const float DefaultY = 0.87f;

		[DefaultValue(typeof(Vector2), "0.89, 0.87")]
		[Label("Max Minion UI Offset")]
		public Vector2 Offset;
    }
}
