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
		public List<ItemModel> history = new List<ItemModel>();

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Selection Book");
			Tooltip.SetDefault("Right click to open the UI"
				+ "\nLeft/Right click on the item icon to select it"
				+ "\nLeft click to use the selected item");
		}

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 30;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Orange;
			Item.mana = 2;
			Item.useAnimation = 16;
			Item.useTime = 16;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item46;
			Item.value = Item.sellPrice(silver: 10);
		}

		public override ModItem Clone(Item item) {
			var clone = (MinionHistoryBookSimple)base.Clone(item);
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

		public override void NetReceive(BinaryReader reader) {
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
			CreateRecipe(1).AddIngredient(ItemID.Book).AddIngredient(ItemID.FallenStar, 6).AddTile(TileID.Bookcases).Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (history.Count > 0) {
				if (Main.LocalPlayer.HasItem(history[0].ItemType)) {
					tooltips.Add(new TooltipLine(Mod, "ItemModel", "Selected: " + history[0].Name));
				}
				else {
					tooltips.Add(new TooltipLine(Mod, "NoneFound", "Selected item not found"));
				}
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "None", "No item specified"));
			}
		}

		public override bool? UseItem(Player player) {
			EnqueueSpawns(player);
			return true;
		}

		public void EnqueueSpawns(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				int count = history.Sum(x => x.Active ? x.SummonCount : 0);
				if (count > 1) {
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile p = Main.projectile[i];
						if (p.active && p.owner == Main.myPlayer && p.minion) {
							p.Kill();
						}
					}
				}

				var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
				mPlayer.pendingCasts.Clear();
				foreach (var item in history) {
					for (int i = 0; i < item.SummonCount; i++) {
						mPlayer.pendingCasts.Enqueue(new Tuple<int, int>(item.ItemType, 1));
					}
				}
			}
		}
	}
}
