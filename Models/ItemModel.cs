using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Models
{
	/// <summary>
	/// Holds the Item data relevant to the book and its index in LocalPlayers inventory
	/// </summary>
	public class ItemModel : TagSerializable, IComparable<ItemModel>
	{
		public static readonly Func<TagCompound, ItemModel> DESERIALIZER = Load;

		/// <summary>
		/// Item type of the item
		/// </summary>
		public int ItemType { get; set; }

		/// <summary>
		/// Display name of the item
		/// </summary>
		public string Name => Lang.GetItemNameValue(ItemType);

		/// <summary>
		/// This is just for sorting on the UI and in the tooltip.
		/// Don't use it for QuickUseItemInSlot, do an ItemType check instead!
		/// </summary>
		public int InventoryIndex { get; set; }

		/// <summary>
		/// Amount of times the item is cast
		/// </summary>
		public byte SummonCount { get; set; }

		/// <summary>
		/// How many minion slots the weapon "creates" on use
		/// </summary>
		public float SlotsFilledPerUse => ItemType > -1 && ItemType < ItemLoader.ItemCount ? ItemID.Sets.StaffMinionSlotsRequired[ItemType] : 1;

		/// <summary>
		///If this ItemModel corresponds to an item in the players inventory
		/// </summary>
		public bool Active { get; set; }

		/// <summary>
		/// Default constructor. Used in Load
		/// </summary>
		public ItemModel() {
			ItemType = 0;
			InventoryIndex = 0;
			SummonCount = 0;
			Active = false;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		public ItemModel(ItemModel itemModel) {
			ItemType = itemModel.ItemType;
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
			InventoryIndex = inventoryIndex;
			SummonCount = summonCount;
			Active = active;
		}

		/// <summary>
		/// Set current itemModel SummonCount to what the loadout had
		/// </summary>
		public void OverrideValuesFromLoadout(ItemModel loadoutModel) {
			SummonCount = loadoutModel.SummonCount;
			Active = true;
		}

		/// <summary>
		/// Set InventoryIndex (sorting criteria) to a high value outside of the inventory array
		/// and deactivate it
		/// </summary>
		public void OverrideValuesToInactive(int i) {
			InventoryIndex = Main.InventorySlotsTotal + i;
			Active = false;
		}

		public override string ToString() =>
			"Name: " + Name + "; Active: " + Active + "; Index: " + InventoryIndex + "; Count: " + SummonCount;

		public TagCompound SerializeData() {
			var item = new Item();
			item.SetDefaults(ItemType);
			return new TagCompound {
				{"item", item },
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
			var item = tag.Get<Item>("item");
			return new ItemModel {
				ItemType = item.type,
				InventoryIndex = tag.GetInt(nameof(InventoryIndex)),
				SummonCount = tag.GetByte(nameof(SummonCount)),
				Active = tag.GetBool(nameof(Active))
			};
		}

		public void NetReceive(BinaryReader reader) {
			ItemType = reader.ReadInt32();
			InventoryIndex = reader.ReadInt32();
			SummonCount = reader.ReadByte();
			Active = reader.ReadBoolean();
		}

		public void NetSend(BinaryWriter writer) {
			writer.Write((int)ItemType);
			writer.Write((int)InventoryIndex);
			writer.Write((byte)SummonCount);
			writer.Write((bool)Active);
		}
	}
}
