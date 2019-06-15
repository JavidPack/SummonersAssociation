using Microsoft.Xna.Framework;
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

		//More fields later, this is temporary
		public int currentMinionWeaponType = 0;
		public string testStringName = "";

		//Something like this?
		//public List<ItemModel> itemModels;

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
			//primitive type, no need to clone
			//clone.currentMinionWeapon = currentMinionWeapon;
			return clone;
		}

		public override TagCompound Save() {
			return new TagCompound {
				{ nameof(currentMinionWeaponType), currentMinionWeaponType },
				{ nameof(testStringName), testStringName }
			};
		}

		public override void Load(TagCompound tag) {
			currentMinionWeaponType = tag.GetInt(nameof(currentMinionWeaponType));
			testStringName = tag.GetString(nameof(testStringName));
		}

		public override void NetRecieve(BinaryReader reader) {
			currentMinionWeaponType = reader.ReadInt32();
			testStringName = reader.ReadString();
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(currentMinionWeaponType);
			writer.Write(testStringName);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (testStringName != null) tooltips.Add(new TooltipLine(mod, "Name",  (testStringName == "") ? "No summon weapon specified" : testStringName) {
				overrideColor = Color.Orange
			});
		}

		public override bool UseItem(Player player) {
			//Here would be code to summon the history
			//clueless how to make it return false and still only run this code once without relying on animation duration
			//if (player.itemTime == 0 && player.itemAnimation == item.useAnimation - 1) Main.NewText(currentMinionWeaponType);
			return false;
		}
	}
}
