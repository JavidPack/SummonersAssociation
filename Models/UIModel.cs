using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SummonersAssociation.Models
{
	public class UIModel
	{
		public Rectangle BackgroundRect { get; set; }
		public Color BackgroundColor { get; set; }
		public Rectangle ItemRect { get; set; }
		public Color ItemColor { get; set; }
		public Color NumberColor { get; set; }
		public List<string> Tooltip { get; set; }
		public string Number { get; set; }
		public int ItemType { get; set; }
		public bool Mouseover { get; set; }

		public UIModel(bool mouseOver, int itemType, Rectangle bgRect, Color bgCol, Rectangle itemRect, Color iCol,
			List<string> tooltip, Color nCol = default, string number = null) {
			Mouseover = mouseOver;
			ItemType = itemType;
			BackgroundRect = bgRect;
			BackgroundColor = bgCol;
			ItemRect = itemRect;
			ItemColor = iCol;
			Tooltip = tooltip;
			//optional
			NumberColor = nCol;
			Number = number;
		}
	}
}