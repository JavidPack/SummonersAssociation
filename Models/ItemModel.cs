using Terraria;

namespace SummonersAssociation.Models
{
	/// <summary>
	/// Holds the Item reference and its index in LocalPlayers inventory
	/// </summary>
	public class ItemModel
	{
		//Item reference needed so it can set the books saved value
		//For Save/Load on the book, need to think of something else
		//More fields later, this is all just temporary
		public Item Item { get; set; }

		public int InventoryIndex { get; set; }

		//unused yet
		public int SummonCount { get; set; }

		public string Name => Item.Name;

		public ItemModel(Item item, int inventoryIndex, int summonCount = 0) {
			Item = item;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
		}

		public override string ToString() =>
			"Name: " + Name + "; Index: " + InventoryIndex + "; Count: " + SummonCount;

		//public ItemModel Clone() => new ItemModel(Item, InventoryIndex, SummonCount);
	}
}
