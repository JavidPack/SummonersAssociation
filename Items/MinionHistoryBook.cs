using Microsoft.Xna.Framework;
using SummonersAssociation.Models;
using SummonersAssociation.UI;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Items
{
	public class MinionHistoryBook : ModItem
	{
		public override bool CloneNewInstances => true;

		public List<ItemModel> history = new List<ItemModel>();

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion History Book");
			//TODO
			Tooltip.SetDefault("TODO");
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 30;
			item.maxStack = 1;
			item.rare = 4;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = 4;
			item.UseSound = SoundID.Item44;
			item.value = Item.sellPrice(silver: 10);
		}

		public override ModItem Clone() {
			var clone = (MinionHistoryBook)base.Clone();
			clone.history = history.ConvertAll((itemModel) => new ItemModel(itemModel));
			return clone;
		}

		public override TagCompound Save() {
			return new TagCompound {
				{nameof(history), history}
			};
		}

		public override void Load(TagCompound tag) {
			var list = tag.GetList<ItemModel>(nameof(history));
			history = new List<ItemModel>(list);
		}

		public override void NetRecieve(BinaryReader reader) {
			int length = reader.ReadByte();
			for (int i = 0; i < length; i++) {
				history[i].NetRecieve(reader);
			}
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)history.Count);
			for (int i = 0; i < history.Count; i++) {
				history[i].NetSend(writer);
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			List<ItemModel> localHistory = HistoryBookUI.MergeHistoryIntoInventory(this);
			if (localHistory.Count > 0) {
				for (int i = 0; i < localHistory.Count; i++) {
					ItemModel itemModel = localHistory[i];
					string name = itemModel.Name;
					int summonCount = itemModel.SummonCount;

					tooltips.Add(new TooltipLine(mod, "ItemModel", name + ": " + summonCount) {
						overrideColor = itemModel.Active ? Color.White : Color.Red
					});
				}
			}
			else {
				tooltips.Add(new TooltipLine(mod, "None", "No summon history specified"));
			}
		}

		public override bool UseItem(Player player) {
			//Here would be code to summon the history
			//clueless how to make it return false and still only run this code once without relying on animation duration
			//if (player.itemTime == 0 && player.itemAnimation == item.useAnimation - 1) Main.NewText(currentMinionWeaponType);
			return false;
		}
	}
}
