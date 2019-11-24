using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SummonersAssociation.Models
{
	public class UIModel
	{
		public Rectangle BackgroundRect { get; set; }
		public Color BackgroundColor { get; set; }
		public Rectangle DestRect { get; set; }
		public Rectangle SourceRect { get; set; }
		public Color ItemColor { get; set; }
		public Color NumberColor { get; set; }
		public List<string> Tooltip { get; set; }
		public string Number { get; set; }
		public int ItemType { get; set; }
		public bool Mouseover { get; set; }

		public UIModel(bool mouseOver, int itemType, Rectangle bgRect, Color bgCol, Rectangle itemRect, Rectangle sourceRect,
			Color iCol, List<string> tooltip, Color nCol, string number) {
			Mouseover = mouseOver;
			ItemType = itemType;
			BackgroundRect = bgRect;
			BackgroundColor = bgCol;
			DestRect = itemRect;
			SourceRect = sourceRect;
			ItemColor = iCol;
			Tooltip = tooltip;
			NumberColor = nCol;
			Number = number;
		}
	}
}