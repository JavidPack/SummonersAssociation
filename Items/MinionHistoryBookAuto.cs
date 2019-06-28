namespace SummonersAssociation.Items
{
	public class MinionHistoryBookAuto : MinionHistoryBook
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Automatic Minion History Book");
			//TODO
			Tooltip.SetDefault("TODO"
				+ "\nLeft click summon minions based on history"
				+ "\nRight click to open an UI"
				+ "\nLeft/Right click on the item icons to adjust the summon count"
				+ "\nAutomatically summons minions when you spawn");
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.rare = 5;
		}

		public override void AddRecipes() {
			//TODO
		}
	}
}
