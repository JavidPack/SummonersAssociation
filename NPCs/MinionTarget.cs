using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.NPCs
{
	//TODO Change reticle color in multiplayer based on team maybe?
	public class MinionTarget : ModNPC {
		//So the NewNPC call centers it
		public const int size = 34;

		public Player Owner => Initialized && PlayerIndex >= 0 && PlayerIndex < Main.maxPlayers ? Main.player[PlayerIndex] : null;

		public int PlayerIndex {
			get => (int)npc.ai[0];
			private set => npc.ai[0] = value;
		}

		public Vector2 Location {
			get => new Vector2(npc.ai[1], npc.ai[2]);
			private set {
				npc.ai[1] = value.X;
				npc.ai[2] = value.Y;
			}
		}

		private const int howdoyouturnthison = int.MaxValue / 5113056;

		public bool Initialized {
			get => npc.ai[3] == howdoyouturnthison;
			private set => npc.ai[3] = value ? howdoyouturnthison : 0;
		}

		public void SetLocation(Point location) => Location = location.ToVector2();

		public void Setup(Player player, Point location) {
			PlayerIndex = player.whoAmI;
			SetLocation(location);
		}

		public void Die() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				npc.life = 0;
				npc.active = false;
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.StrikeNPC, number: npc.whoAmI, number2: -1f);
				}
				//Console.WriteLine("killed " + npc.whoAmI);
				//Main.NewText("killed " + npc.whoAmI);
			}
		}

		private bool IsValid() {
			//The following "valid" checks are made only serverside
			if (Location == Vector2.Zero) {
				//Not a genuine target, delete itself
				//Console.WriteLine("not genuine");
				//Main.NewText("not genuine");
				return false;
			}

			if (Owner == null || !Owner.active || Owner.dead) {
				//Console.WriteLine("die cause owner is null, not active or dead");
				//Main.NewText("die cause owner is null, not active or dead");
				return false;
			}

			if (Owner != null) {
				var mPlayer = Owner.GetModPlayer<SummonersAssociationPlayer>();
				NPC target = mPlayer.Target;

				if (target == null) {
					return true;
				}

				int whoAmI = target.whoAmI;
				if (whoAmI != npc.whoAmI || whoAmI != Owner.MinionAttackTargetNPC) {
					//Console.WriteLine("die cause player target whoami doesn't match this dtargetummy");
					//Main.NewText("die cause player target whoami doesn't match this target");
					return false;
				}
			}
			return true;
		}

		public override void SetStaticDefaults() => DisplayName.SetDefault("Minion Target");
		
		public override void SetDefaults() {
			npc.width = size;
			npc.height = size;
			npc.defense = 0;
			npc.damage = 0;
			npc.aiStyle = 0;
			npc.noGravity = true;
			npc.lavaImmune = true;
			npc.noTileCollide = true;
			npc.netAlways = true;
			for (int i = 0; i < BuffLoader.BuffCount; i++) {
				npc.buffImmune[i] = true;
			}

			//Flag it as "chaseable", because the threshold is 5
			npc.lifeMax = 6;

			//Gets set to false just before a projectile does its AI
			//friendly to true avoids a few unintended interactions like being able to dash the target, or the target picking up coins
			npc.friendly = true;

			drawOffsetY = -4;
		}

		public override void AI() {
			if (!Initialized) {
				Initialized = true;
				//Console.WriteLine("init");
				//Main.NewText("init");
			}

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				bool valid = IsValid();
				if (!valid) {
					Die();
					return;
				}
			}

			npc.Center = Location;
			npc.visualOffset = Vector2.Zero;
			npc.timeLeft = 10;

			//Change name
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				npc.GivenName = $"{npc.TypeName} ({Owner?.name})";
			}
			else if (Main.netMode == NetmodeID.SinglePlayer) {
				npc.GivenName = " ";
			}

			//Change alpha of targets from other players
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				if (npc.alpha != 255) {
					if (Main.myPlayer != PlayerIndex && Main.LocalPlayer.MinionAttackTargetNPC != npc.whoAmI) {
						npc.alpha = 100;
					}
					else {
						npc.alpha = 0;
					}
				}
			}
		}

		public override Color? GetAlpha(Color drawColor) => Color.White * npc.Opacity;

		public override bool? CanBeHitByItem(Player player, Item item) => false;

		public override bool? CanBeHitByProjectile(Projectile projectile) => false;

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

		public override bool? CanHitNPC(NPC target) => false;

		//Safety net for whatever
		//Cheatsheet butcher, other outside-of-projectile-ai direct damage applications
		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit) {
			damage = 0;
			return false;
		}
	}
}
