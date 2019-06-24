using System;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Models
{
	/// <summary>
	/// Holds the Item reference and its index in LocalPlayers inventory
	/// </summary>
	public class ItemModel : TagSerializable, IComparable<ItemModel>
	{
		public static readonly Func<TagCompound, ItemModel> DESERIALIZER = Load;

		public int ItemType { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// This is just for sorting on the UI and in the tooltip.
		/// dont use it for QuickUseItemInSlot(), do an ItemType check instead!
		/// </summary>
		public int InventoryIndex { get; set; }

		public byte SummonCount { get; set; }

		/// <summary>
		///If this ItemModel corresponds to an item in the players inventory
		/// </summary>
		public bool Active { get; set; }

		/// <summary>
		/// Default constructor. Is it needed?
		/// </summary>
		public ItemModel() {
			ItemType = 0;
			Name = "";
			InventoryIndex = 0;
			SummonCount = 0;
			Active = false;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public ItemModel(ItemModel itemModel) {
			ItemType = itemModel.ItemType;
			Name = itemModel.Name;
			InventoryIndex = itemModel.InventoryIndex;
			SummonCount = itemModel.SummonCount;
			Active = itemModel.Active;
		}

		/// <summary>
		/// Convenience constructor
		/// </summary>
		public ItemModel(Item item, int inventoryIndex, byte summonCount = 0, bool active = true) {
			//If created from an item, it is by definition active
			ItemType = item.type;
			Name = item.Name;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
			Active = active;
		}

		/// <summary>
		/// Proper constructor
		/// </summary>
		public ItemModel(int itemType, string name, int inventoryIndex, byte summonCount = 0, bool active = false) {
			ItemType = itemType;
			Name = name;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
			Active = active;
		}

		/// <summary>
		/// Set current itemModel SummonCount to what the history had
		/// </summary>
		public void OverrideValuesFromHistory(ItemModel historyModel) {
			SummonCount = historyModel.SummonCount;
			Active = true;
		}

		/// <summary>
		/// Set InventoryIndex (sorting criteria) to a high value outside of the inventory array
		/// and deactivate it
		/// </summary>
		public void OverrideValuesToInactive(int i) {
			InventoryIndex = Main.maxInventory + i;
			Active = false;
		}

		public override string ToString() =>
			"Name: " + Name + "; Active: " + Active + "; Index: " + InventoryIndex + "; Count: " + SummonCount;

		public TagCompound SerializeData() {
			return new TagCompound {
				{nameof(ItemType), ItemType },
				{nameof(Name), Name },
				{nameof(InventoryIndex), InventoryIndex },
				{nameof(SummonCount), SummonCount },
				{nameof(Active), Active },
			};
		}

		/// <summary>
		/// Sorted by InventoryIndex
		/// </summary>
		public int CompareTo(ItemModel other) => InventoryIndex.CompareTo(other.InventoryIndex);

		public static ItemModel Load(TagCompound tag) {
			return new ItemModel {
				ItemType = tag.GetInt(nameof(ItemType)),
				Name = tag.GetString(nameof(Name)),
				InventoryIndex = tag.GetInt(nameof(InventoryIndex)),
				SummonCount = tag.GetByte(nameof(SummonCount)),
				Active = tag.GetBool(nameof(Active))
			};
		}

		public void NetRecieve(BinaryReader reader) {
			ItemType = reader.ReadInt32();
			Name = reader.ReadString();
			InventoryIndex = reader.ReadInt32();
			SummonCount = reader.ReadByte();
			Active = reader.ReadBoolean();
		}

		public void NetSend(BinaryWriter writer) {
			writer.Write((int)ItemType);
			writer.Write((string)Name);
			writer.Write((int)InventoryIndex);
			writer.Write((byte)SummonCount);
			writer.Write((bool)Active);
		}
	}
}
