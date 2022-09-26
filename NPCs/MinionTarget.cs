using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonersAssociation.NPCs
{
	//TODO Change reticle color in multiplayer based on team maybe?
	public class MinionTarget : ModNPC
	{
		//So the NewNPC call centers it
		public const int size = 34;

		public Player Owner => Initialized && PlayerIndex >= 0 && PlayerIndex < Main.maxPlayers ? Main.player[PlayerIndex] : null;

		public int PlayerIndex {
			get => (int)NPC.ai[0];
			private set => NPC.ai[0] = value;
		}

		public Vector2 Location {
			get => new Vector2(NPC.ai[1], NPC.ai[2]);
			private set {
				NPC.ai[1] = value.X;
				NPC.ai[2] = value.Y;
			}
		}

		private const int howdoyouturnthison = int.MaxValue / 5113056;

		public bool Initialized {
			get => NPC.ai[3] == howdoyouturnthison;
			private set => NPC.ai[3] = value ? howdoyouturnthison : 0;
		}

		public void SetLocation(Point location) => Location = location.ToVector2();

		public void Setup(Player player, Point location) {
			PlayerIndex = player.whoAmI;
			SetLocation(location);
		}

		public void Die() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.life = 0;
				NPC.active = false;
				if (Main.netMode == NetmodeID.Server) {
					NetMessage.SendData(MessageID.DamageNPC, number: NPC.whoAmI, number2: -1f);
				}
				//Console.WriteLine("killed " + NPC.whoAmI);
				//Main.NewText("killed " + NPC.whoAmI);
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
				if (whoAmI != NPC.whoAmI || whoAmI != Owner.MinionAttackTargetNPC) {
					//Console.WriteLine("die cause player target whoami doesn't match this target");
					//Main.NewText("die cause player target whoami doesn't match this target");
					return false;
				}
			}
			return true;
		}

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Target");

			NPCID.Sets.NPCBestiaryDrawOffset[NPC.type] = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true, //Hides this NPC from the Bestiary
			};

			NPCID.Sets.DebuffImmunitySets[NPC.type] = new Terraria.DataStructures.NPCDebuffImmunityData() {
				ImmuneToAllBuffsThatAreNotWhips = true,
				ImmuneToWhips = true
			};
		}

		public override void SetDefaults() {
			NPC.width = size;
			NPC.height = size;
			NPC.defense = 0;
			NPC.damage = 0;
			NPC.aiStyle = 0;
			NPC.noGravity = true;
			NPC.lavaImmune = true;
			NPC.noTileCollide = true;
			NPC.netAlways = true;
			NPC.dontTakeDamageFromHostiles = true;
			NPC.npcSlots = 0f;

			//Flag it as "chaseable", because the threshold is 5
			NPC.lifeMax = 6;

			//Gets set to false just before a projectile does its AI
			//friendly to true avoids a few unintended interactions like being able to dash the target, or the target picking up coins
			NPC.friendly = true;

			DrawOffsetY = -4;
		}

		public override bool CheckActive() => !ServerConfig.Instance.PersistentReticle;

		public override void AI() {
			if (ServerConfig.Instance.DisableAdvancedTargetingFeature) {
				NPC.active = false;
				return;
			}

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

			NPC.Center = Location;
			//NPC.visualOffset = Vector2.Zero;
			NPC.timeLeft = 120;

			//Change name
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NPC.GivenName = $"{NPC.TypeName} ({Owner?.name})";
			}
			else if (Main.netMode == NetmodeID.SinglePlayer) {
				NPC.GivenName = " ";
			}

			//Change alpha of targets from other players
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				if (NPC.alpha != 255) {
					if (Main.myPlayer != PlayerIndex && Main.LocalPlayer.MinionAttackTargetNPC != NPC.whoAmI) {
						NPC.alpha = 100;
					}
					else {
						NPC.alpha = 0;
					}
				}
			}
		}

		public override Color? GetAlpha(Color drawColor) => Color.White * NPC.Opacity;

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
