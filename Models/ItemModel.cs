using System;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Models
{
	/// <summary>
	/// Holds the Item reference and its index in LocalPlayers inventory
	/// </summary>
	public class ItemModel : TagSerializable
	{
		public static readonly Func<TagCompound, ItemModel> DESERIALIZER = Load;

		//More fields later, this is all just temporary
		public int ItemType { get; set; }

		public string Name { get; set; }

		public int InventoryIndex { get; set; }

		//unused yet, will only get drawn
		public byte SummonCount { get; set; }

		public ItemModel() {
			ItemType = 0;
			Name = "";
			InventoryIndex = 0;
			SummonCount = 0;
		}

		public ItemModel(Item item, int inventoryIndex, byte summonCount = 0) {
			ItemType = item.type;
			Name = item.Name;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
		}

		public ItemModel(int itemType, string name, int inventoryIndex, byte summonCount = 0) {
			ItemType = itemType;
			Name = name;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
		}

		public override string ToString() =>
			"Name: " + Name + "; Index: " + InventoryIndex + "; Count: " + SummonCount;

		public TagCompound SerializeData() {
			return new TagCompound {
				{nameof(ItemType), ItemType },
				{nameof(Name), Name },
				{nameof(InventoryIndex), InventoryIndex },
				{nameof(SummonCount), SummonCount },
			};
		}
		public static ItemModel Load(TagCompound tag) {
			return new ItemModel {
				ItemType = tag.GetInt(nameof(ItemType)),
				Name = tag.GetString(nameof(Name)),
				InventoryIndex = tag.GetInt(nameof(InventoryIndex)),
				SummonCount = tag.GetByte(nameof(SummonCount))
			};
		}

		public void NetRecieve(BinaryReader reader) {
			ItemType = reader.ReadInt32();
			Name = reader.ReadString();
			InventoryIndex = reader.ReadInt32();
			SummonCount = reader.ReadByte();
		}

		public void NetSend(BinaryWriter writer) {
			writer.Write((int)ItemType);
			writer.Write((string)Name);
			writer.Write((int)InventoryIndex);
			writer.Write((byte)SummonCount);
		}

		public ItemModel Clone() => new ItemModel(ItemType, Name, InventoryIndex, SummonCount);
	}
}
