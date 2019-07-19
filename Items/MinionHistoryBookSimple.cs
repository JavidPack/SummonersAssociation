using SummonersAssociation.Models;
using System;
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
			Tooltip.SetDefault("Right click to open the UI"
				+ "\nLeft/Right click on the item icon to select it"
				+ "\nLeft click to summon the selected item");
		}

		public override void SetDefaults() {
			item.width = 28;
			item.height = 30;
			item.maxStack = 1;
			item.rare = 3;
			item.mana = 2;
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
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Book);
			recipe.AddIngredient(ItemID.BlackInk);
			recipe.AddIngredient(ItemID.FallenStar, 6);
			recipe.AddTile(TileID.Chairs);
			recipe.AddTile(TileID.Tables);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (history.Count > 0) {
				if (Main.LocalPlayer.HasItem(history[0].ItemType)) {
					tooltips.Add(new TooltipLine(mod, "ItemModel", "Selected: " + history[0].Name));
				}
				else {
					tooltips.Add(new TooltipLine(mod, "NoneFound", "Selected item not found"));
				}
			}
			else {
				tooltips.Add(new TooltipLine(mod, "None", "No item specified"));
			}
		}

		public override bool UseItem(Player player) {
			EnqueueSpawns(player);
			return true;
		}

		public void EnqueueSpawns(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				int count = history.Sum(x => x.SummonCount);
				if (count > 1) {
					for (int i = 0; i < 1000; i++) {
						Projectile p = Main.projectile[i];
						if (p.active && p.owner == Main.myPlayer && p.minion) {
							p.Kill();
						}
					}
				}

				var SAPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
				SAPlayer.pendingCasts.Clear();
				foreach (var item in history) {
					for (int i = 0; i < item.SummonCount; i++) {
						SAPlayer.pendingCasts.Enqueue(new Tuple<int, int>(item.ItemType, 1));
					}
				}
			}
		}
	}
}
