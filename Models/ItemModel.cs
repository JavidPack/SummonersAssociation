using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Models
{
	/// <summary>
	/// Holds the Item reference and its index in LocalPlayers inventory
	/// </summary>
	public class ItemModel : TagSerializable, IComparable<ItemModel>
	{
		public static readonly Func<TagCompound, ItemModel> DESERIALIZER = Load;

		public const string VANILLA = "Terraria";

		/// <summary>
		/// This is set in Load() for ModName when the mod is unloaded
		/// </summary>
		public const string UNLOADED = "fhs8tshf8rogz6ofruigdh4";

		public int ItemType { get; set; }

		public string Name { get; set; }

		/// <summary>
		/// The mod name of the item. "Terraria" if not a modded item
		/// </summary>
		public string ModName { get; set; }

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
			ModName = VANILLA;
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
			ModName = itemModel.ModName;
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
			ModName = item.modItem != null? item.modItem.mod.Name : VANILLA;
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
			Active = active;
		}

		/// <summary>
		/// Proper constructor, not used anywhere
		/// </summary>
		public ItemModel(int itemType, string name, string modName, int inventoryIndex, byte summonCount = 0, bool active = false) {
			ItemType = itemType;
			Name = name;
			ModName = modName;
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
				{nameof(ModName), ModName },
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
			ItemModel itemModel = new ItemModel {
				ItemType = tag.GetInt(nameof(ItemType)),
				Name = tag.GetString(nameof(Name)),
				ModName = tag.GetString(nameof(ModName)),
				InventoryIndex = tag.GetInt(nameof(InventoryIndex)),
				SummonCount = tag.GetByte(nameof(SummonCount)),
				Active = tag.GetBool(nameof(Active))
			};
			if (itemModel.ModName != VANILLA) {
				if (ModLoader.GetMod(itemModel.ModName) == null) {
					itemModel.ModName = UNLOADED;
				}
			}
			return itemModel;
		}

		public void NetRecieve(BinaryReader reader) {
			ItemType = reader.ReadInt32();
			Name = reader.ReadString();
			ModName = reader.ReadString();
			InventoryIndex = reader.ReadInt32();
			SummonCount = reader.ReadByte();
			Active = reader.ReadBoolean();
		}

		public void NetSend(BinaryWriter writer) {
			writer.Write((int)ItemType);
			writer.Write((string)Name);
			writer.Write((string)ModName);
			writer.Write((int)InventoryIndex);
			writer.Write((byte)SummonCount);
			writer.Write((bool)Active);
		}
	}
}
