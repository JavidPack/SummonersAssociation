using SummonersAssociation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace SummonersAssociation.Items
{
	/// <summary>
	/// This is the base class for the other minion books, and also acts like one
	/// (for this one, loadout only has max one element in it, and functionality is different)
	/// </summary>
	[LegacyName("MinionHistoryBookSimple")]
	public class MinionLoadoutBookSimple : ModItem
	{
		public List<ItemModel> loadout = new List<ItemModel>();

		public static LocalizedText SelectedNotInInventoryText { get; private set; }
		public static LocalizedText NoSelectedText { get; private set; }

		public override void SetStaticDefaults() {
			//Localizations here should be "static" since this class is inherited from
			string category = $"{ModContent.GetInstance<MinionLoadoutBookSimple>().LocalizationCategory}.{nameof(MinionLoadoutBookSimple)}.";
			SelectedNotInInventoryText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}SelectedNotInInventory"));
			NoSelectedText ??= Language.GetOrRegister(Mod.GetLocalizationKey($"{category}NoSelected"));
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
			var clone = (MinionLoadoutBookSimple)base.Clone(item);
			clone.loadout = loadout.ConvertAll((itemModel) => new ItemModel(itemModel));
			return clone;
		}

		public override void SaveData(TagCompound tag) {
			tag.Add("history", loadout);
		}

		public override void LoadData(TagCompound tag) {
			//Load and remove unloaded items from loadout
			loadout = tag.GetList<ItemModel>("history").Where(IsNotUnloadedItem).ToList();
		}

		private static bool IsNotUnloadedItem(ItemModel x) {
			return ItemLoader.GetItem(x.ItemType) is not UnloadedItem;
		}

		public override void NetReceive(BinaryReader reader) {
			int length = reader.ReadByte();
			loadout = new List<ItemModel>();
			for (int i = 0; i < length; i++) {
				loadout.Add(new ItemModel());
				loadout[i].NetReceive(reader);
			}
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)loadout.Count);
			for (int i = 0; i < loadout.Count; i++) {
				loadout[i].NetSend(writer);
			}
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemID.Book).AddIngredient(ItemID.FallenStar, 6).AddTile(TileID.Bookcases).Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (loadout.Count > 0) {
				if (Main.LocalPlayer.HasItem(loadout[0].ItemType)) {
					tooltips.Add(new TooltipLine(Mod, "ItemModel", UISystem.LoadoutBookOnUseSelected.Format(loadout[0].Name)));
				}
				else {
					tooltips.Add(new TooltipLine(Mod, "NoneFound", SelectedNotInInventoryText.ToString()));
				}
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "None", NoSelectedText.ToString()));
			}
		}

		public override bool? UseItem(Player player) {
			EnqueueSpawns(player);
			return true;
		}

		public void EnqueueSpawns(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				int count = loadout.Sum(x => x.Active ? x.SummonCount : 0);
				bool canKillMinions = count >= 1;
				if (Array.IndexOf(SummonersAssociation.BookTypes, player.HeldItem.type) == 0) {
					canKillMinions = false;
				}
				if (canKillMinions) {
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile p = Main.projectile[i];
						if (p.active && p.owner == Main.myPlayer && p.minion) {
							if (SummonersAssociation.SupportedMinions.FirstOrDefault(m => m.ContainsProjID(p.type)) is MinionModel model)
								player.ClearBuff(model.BuffID); // This makes sure minions spawned from buffs properly despawn (Finch Staff)

							p.Kill();
						}
					}
				}
			}

			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();
			mPlayer.pendingCasts.Clear();
			foreach (var item in loadout) {
				for (int i = 0; i < item.SummonCount; i++) {
					mPlayer.pendingCasts.Enqueue(new Tuple<int, int>(item.ItemType, 1));
				}
			}
		}
	}
}
