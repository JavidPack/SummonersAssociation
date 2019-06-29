using SummonersAssociation.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Items
{
	/// <summary>
	/// This is the base class for the other minion books, and also acts like one
	/// (for this one, history only has max one element in it, and functionality is different)
	/// </summary>
	public class MinionHistoryBookSimple : ModItem
	{
		public override bool CloneNewInstances => true;

		public List<ItemModel> history = new List<ItemModel>();

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Selection Book");
			//TODO
			Tooltip.SetDefault("TODO"
				+ "\nRight click to open an UI"
				+ "\nLeft click on the item icon to set item"
				+ "\nLeft click to summon the selected item");
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 30;
			item.maxStack = 1;
			item.rare = 3;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = 4;
			item.UseSound = SoundID.Item46;
			item.value = Item.sellPrice(silver: 10);
		}

		public override ModItem Clone() {
			var clone = (MinionHistoryBookSimple)base.Clone();
			clone.history = history.ConvertAll((itemModel) => new ItemModel(itemModel));
			return clone;
		}

		public override TagCompound Save() {
			return new TagCompound {
				{nameof(history), history}
			};
		}

		public override void Load(TagCompound tag) {
			//Load and remove unloaded items from history
			history = tag.GetList<ItemModel>(nameof(history)).Where(x => x.ItemType != ItemID.Count).ToList();
		}

		public override void NetRecieve(BinaryReader reader) {
			int length = reader.ReadByte();
			history = new List<ItemModel>();
			for (int i = 0; i < length; i++) {
				history.Add(new ItemModel());
				history[i].NetRecieve(reader);
			}
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)history.Count);
			for (int i = 0; i < history.Count; i++) {
				history[i].NetSend(writer);
			}
		}

		public override void AddRecipes() {
			//TODO
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (history.Count > 0) {
				if (Main.LocalPlayer.HasItem(history[0].ItemType)) {
					tooltips.Add(new TooltipLine(mod, "ItemModel", "Specified: " + history[0].Name));
				}
				else {
					tooltips.Add(new TooltipLine(mod, "NoneFound", "Specified item not found"));
				}
			}
			else {
				tooltips.Add(new TooltipLine(mod, "None", "No item specified"));
			}
		}

		public override bool UseItem(Player player) {
			if (history.Count > 0) {
				var SAPlayer = player.GetModPlayer<SAPlayer>();
				//Will fail if no item found
				SAPlayer.QuickUseItemOfType(history[0].ItemType);
			}
			return true;
		}
	}
}
