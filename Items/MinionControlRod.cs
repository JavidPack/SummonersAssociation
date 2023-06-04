using Microsoft.Xna.Framework;
using SummonersAssociation.NPCs;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Audio;
using Terraria.DataStructures;
using MonoMod.RuntimeDetour;
using Terraria.Localization;

namespace SummonersAssociation.Items
{
	public class MinionControlRod : ModItem
	{
		public static LocalizedText Rightclick1Text { get; private set; }
		public static LocalizedText Rightclick2RemoveText { get; private set; }
		public static LocalizedText Rightclick2SpawnText { get; private set; }
		public static LocalizedText Rightclick3Text { get; private set; }

		public override void SetStaticDefaults() {
			Rightclick1Text = this.GetLocalization("Rightclick1");
			Rightclick2RemoveText = this.GetLocalization("Rightclick2Remove");
			Rightclick2SpawnText = this.GetLocalization("Rightclick2Spawn");
			Rightclick3Text = this.GetLocalization("Rightclick3");
		}

		public override void SetDefaults() {
			Item.width = 42;
			Item.height = 42;
			Item.maxStack = 1;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTurn = true;
			//Item.UseSound = SoundID.Item6?.WithVolume(0.6f);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			int insertIndex = -1;
			for (int i = tooltips.Count - 1; i >= 0; i--) {
				if (tooltips[i].Name.StartsWith("Tooltip")) {
					insertIndex = i;
					break;
				}
			}

			if (insertIndex != -1) {
				tooltips.Insert(++insertIndex, new TooltipLine(Mod, "Rightclick1", Rightclick1Text.ToString()));

				if (!ServerConfig.Instance.DisableAdvancedTargetingFeature) {
					string text;
					if (Main.LocalPlayer.GetModPlayer<SummonersAssociationPlayer>().TargetWhoAmI > -1) {
						text = Rightclick2RemoveText.ToString();
					}
					else {
						text = Rightclick2SpawnText.ToString();
					}
					tooltips.Insert(++insertIndex, new TooltipLine(Mod, "Rightclick2", text));

					if (Main.netMode == NetmodeID.MultiplayerClient) {
						tooltips.Insert(++insertIndex, new TooltipLine(Mod, "Rightclick3", Rightclick3Text.ToString()));
					}
				}
			}
		}

		public override void AddRecipes() {
			CreateRecipe(1).AddIngredient(ItemID.FallenStar, 10).AddRecipeGroup(SummonersAssociationSystem.MagicMirrorRecipeGroup).AddTile(TileID.Anvils).Register();
		}

		public override bool AltFunctionUse(Player player) => true;

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			player.itemLocation.X += player.direction * (-16 + 2);
			player.itemLocation.Y += -2;
			int totalTime = CombinedHooks.TotalUseTime(Item.useTime, player, Item);

			if (player.altFunctionUse != 2) {
				//This runs for all clients, not the server
				if (player.itemTime == 0) {
					SoundEngine.PlaySound(SoundID.Item6 with { Volume = 0.6f }, player.Center); //Magic mirror sound
					player.itemTime = totalTime;
				}
				if (player.itemTime == totalTime / 2) {
					TeleportAllMinions(player, Main.MouseWorld);
				}
			}
			else if (player.altFunctionUse == 2) {
				//This runs for just the client using it
				if (player.itemTime == 0) {
					player.itemTime = totalTime;
					SoundEngine.PlaySound(SoundID.Item66 with { Volume = 0.6f }, player.Center); //Common minion staff sound

					DoRightClick(player);
				}
			}
		}

		private void TeleportAllMinions(Player player, Vector2 location) {
			bool teleAny = false;
			for (int j = 0; j < Main.maxProjectiles; j++) {
				Projectile projectile = Main.projectile[j];
				if (projectile.active && projectile.owner == player.whoAmI) {
					if (projectile.minion && (!SummonersAssociation.TeleportConditionMinions.TryGetValue(projectile.type, out var func) || func?.Invoke(projectile) == true)) {
						for (int i = 0; i < 40; i++) {
							int index = Dust.NewDust(projectile.position, projectile.width, projectile.height, 14, 0f, 0f, 150, default(Color), 1.1f);
							Dust dust = Main.dust[index];
							dust.noLight = true;
						}

						if (Main.myPlayer == player.whoAmI) {
							projectile.Center = location;
							projectile.netUpdate = true;
							teleAny = true; //Here, otherwise we have to sync mouse cursor position, because that dust effect shows on cursor pos
						}
					}
				}
			}

			if (teleAny) { //Only true clientside
				for (int i = 0; i < 40; i++) {
					var size = new Vector2(26);
					Vector2 pos = location - size / 2;
					int index = Dust.NewDust(pos, (int)size.X, (int)size.Y, 135, 0f, 0f, 150, default(Color), 1.1f);
					Dust dust = Main.dust[index];
					dust.velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1f, 0.2f));
					dust.noGravity = true;
					dust.fadeIn = 1.4f;
					dust.noLight = true;
				}
			}
		}

		private void DoRightClick(Player player) {
			if (Main.myPlayer == player.whoAmI) { //Technically redundant check
				if (ServerConfig.Instance.DisableAdvancedTargetingFeature) {
					//If this feature is disabled, simply treat it as a regular summon weapon right click
					player.MinionNPCTargetAim(false);
					return;
				}

				var location = Main.MouseWorld.ToPoint();
				bool mouseover = false;
				bool mouseoverOwnTarget = false;

				int type = NPCType<MinionTarget>();

				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (CanBeChasedBy(npc, type)) {
						Rectangle hitbox = npc.Hitbox;
						hitbox.Inflate(5, 5);
						if (hitbox.Contains(location)) {
							//Mouseovered an existing NPC that can be targeted: manually retarget nearest to non-target, and delete target
							mouseover = true;

							//If in addition, mouseovered a self-owned target, delete it
							if (npc.type == type) {
								var target = npc.ModNPC as MinionTarget;
								if (target.PlayerIndex == player.whoAmI) {
									mouseoverOwnTarget = true;
								}
							}
							break;
						}
					}
				}
				SpawnTargetWrapper(player, location, mouseover, mouseoverOwnTarget);
			}
		}

		#region Static methods
		internal static void DeleteTarget(Player player) {
			//Clear all player owned targets
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && npc.ModNPC is MinionTarget target && target?.Owner == player) {
					//Console.WriteLine($"[{Main.time}] killed target due to clear");
					//Main.NewText($"[{Main.time}] killed target due to clear");
					target.Die();
				}
			}
		}

		/// <summary>
		/// Same as <seealso cref="NPC.CanBeChasedBy"/>, but takes an additional type check that will also return true retardless of state
		/// </summary>
		internal static bool CanBeChasedBy(NPC npc, int type, bool ignoreDontTakeDamage = false) => npc.CanBeChasedBy(null, ignoreDontTakeDamage) || (npc.active && npc.type == type);

		internal static void SpawnTargetWrapper(Player player, Point location, bool mouseover, bool mouseoverOwnTarget) {
			//Main.NewText($"m:{mouseover}, mot:{mouseoverOwnTarget}");
			if (Main.netMode == NetmodeID.SinglePlayer) {
				SpawnTarget(player, location, mouseover, mouseoverOwnTarget);
			}
			else if (Main.netMode == NetmodeID.MultiplayerClient) {
				SendSpawnTarget(player, location, mouseover, mouseoverOwnTarget);
			}
		}

		internal static void MinionNPCTargetAim(Player player) {
			//Could use player.MinionNPCTargetAim() instead but that doesn't have a check for ignoring some npc
			//ignore is needed because the targeting clientside happens independently of having received data about targets existing yet or not
			Vector2 mouseWorld = Main.MouseWorld;
			int type = NPCType<MinionTarget>();
			int found = -1;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (CanBeChasedBy(npc, type) && (found == -1 || npc.Hitbox.Distance(mouseWorld) < Main.npc[found].Hitbox.Distance(mouseWorld))) {
					if (npc.type == type) {
						var target = npc.ModNPC as MinionTarget;
						if (target.PlayerIndex == player.whoAmI) {
							continue; //Continue if own target
						}
					}
					found = i;
				}
			}

			if (player.MinionAttackTargetNPC == found) //Already the same: deselect
				player.MinionAttackTargetNPC = -1;
			else
				player.MinionAttackTargetNPC = found;
		}

		internal static void SpawnTarget(Player player, Point location, bool mouseover, bool mouseoverOwnTarget) {
			/*
			 * //1) Didn't mouseover, has a  target: Reposition
			 * //2) Didn't mouseover, has no target: Spawn one
			 * //3) Mouseover !target, has a  target: Let client retarget, then delete
			 * //4) Mouseover target , has a  target: Delete
			*/

			if (Main.netMode == NetmodeID.Server) {
				//Console.WriteLine($"[{Main.time}] spawn target {location}, {mouseover}, {mouseoverOwnTarget}");
			}
			else {
				//Main.NewText($"[{Main.time}] spawn target {location}, {mouseover}, {mouseoverOwnTarget}");
			}

			var mPlayer = player.GetModPlayer<SummonersAssociationPlayer>();

			int type = NPCType<MinionTarget>();

			bool hasTarget = mPlayer.HasValidTarget;

			if (!mouseover) {
				if (hasTarget) {
					//1) Didn't mouseover, has a target: Reposition
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						NPC npc = mPlayer.Target;
						var target = npc.ModNPC as MinionTarget;
						target.SetLocation(location);
						if (Main.netMode == NetmodeID.Server) {
							NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
						}
					}
				}
				else {
					//2) Didn't mouseover, has no target: Spawn one
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						int index = NPC.NewNPC(new EntitySource_Parent(player), location.X, location.Y + MinionTarget.size / 2, type);

						if (index < Main.maxNPCs) {
							NPC npc = Main.npc[index];

							var target = npc.ModNPC as MinionTarget;
							target.Setup(player, location);

							mPlayer.PendingTargetAssignment = index;

							//Console.WriteLine("spawned " + index);
							//Main.NewText("spawned " + index);

							if (Main.netMode == NetmodeID.Server) {
								//Console.WriteLine($"Send relevant npc data: ai0:{npc.ai[0]}, ai3:{npc.ai[3]}, style:{npc.aiStyle}");
								NetMessage.SendData(MessageID.SyncNPC, number: index);
								ConfirmTargetToClient(player, index);
							}
						}
					}
				}
			}
			else { //if (mouseover)
				if (!mouseoverOwnTarget) {
					//3) Mouseover !target: Let client retarget, and if has target, delete
					if (Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer) {
						//In multiplayer, this is the only thing the client runs really
						MinionNPCTargetAim(player);
					}
				}

				if (hasTarget) {
					if (mouseoverOwnTarget) {
						//4) Mouseover target, has a target: Delete
					}

					if (Main.netMode != NetmodeID.MultiplayerClient) {
						DeleteTarget(player);
					}
				}
			}
		}

		/// <summary>
		/// Client to server initially, then the server resents to that client
		/// </summary>
		internal static void SendSpawnTarget(Player player, Point location, bool mouseover, bool mouseoverOwnTarget, int toClient = -1) {
			if (Main.netMode == NetmodeID.Server) {
				//Console.WriteLine($"[{Main.time}]sending spawn target");
			}
			else {
				//Main.NewText($"[{Main.time}]request spawn target");
			}

			ModPacket packet = SummonersAssociation.Instance.GetPacket();
			packet.Write((byte)PacketType.SpawnTarget);
			packet.Write((byte)player.whoAmI);
			packet.WriteVector2(location.ToVector2());
			packet.Write(new BitsByte(mouseover, mouseoverOwnTarget));
			packet.Send(toClient);
		}

		internal static void HandleSpawnTarget(BinaryReader reader) {
			if (Main.netMode == NetmodeID.Server) {
				//Console.WriteLine($"[{Main.time}]receive spawn target");
			}
			else {
				//Main.NewText($"[{Main.time}]receive spawn target");
			}

			byte whoAmI = reader.ReadByte();
			Player player = Main.player[whoAmI];

			var location = reader.ReadVector2().ToPoint();

			BitsByte flags = reader.ReadByte();
			bool mouseover = flags[0];
			bool mouseoverOwnTarget = flags[1];

			SpawnTarget(player, location, mouseover, mouseoverOwnTarget);
			if (Main.netMode == NetmodeID.Server) {
				SendSpawnTarget(player, location, mouseover, mouseoverOwnTarget, player.whoAmI);
			}
		}

		/// <summary>
		/// Server to client
		/// </summary>
		internal static void ConfirmTargetToClient(Player player, int npcWhoAmI) {
			ModPacket packet = SummonersAssociation.Instance.GetPacket();
			packet.Write((byte)PacketType.ConfirmTargetToClient);
			packet.Write((byte)npcWhoAmI);
			packet.Send(player.whoAmI);

			if (Main.netMode == NetmodeID.Server) {
				//Console.WriteLine($"[{Main.time}]sending npcwhoami confirm " + npcWhoAmI);
			}
			else {
				//Main.NewText("receive spawn target");
			}
		}

		/// <summary>
		/// Only the local player needs to have PendingTargetAssignment assigned
		/// </summary>
		internal static void HandleConfirmTargetToClient(BinaryReader reader) {
			byte npcWhoAmI = reader.ReadByte();
			var mPlayer = Main.LocalPlayer.GetModPlayer<SummonersAssociationPlayer>();
			mPlayer.PendingTargetAssignment = npcWhoAmI;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				//Main.NewText($"[{Main.time}]receiving npcwhoami confirm " + npcWhoAmI);
				//NPC npc = Main.npc[npcWhoAmI];
				//Main.NewText($"Relevant npc data: ai0:{npc.ai[0]}, ai3:{npc.ai[3]}, style:{npc.aiStyle}");
			}
		}

		/// <summary>
		/// Called in SummonersAssociationPlayer.PostUpdate. Assigns the target to the pending one
		/// </summary>
		internal static void PendingTargetAssignment(SummonersAssociationPlayer mPlayer) {
			if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != mPlayer.Player.whoAmI) return;
			if (ServerConfig.Instance.DisableAdvancedTargetingFeature) return;
			//Only the server and the client owning it will run the code here

			int whoAmI = mPlayer.PendingTargetAssignment;
			if (whoAmI > -1 && whoAmI < Main.maxNPCs) {
				//NPC npc = Main.npc[whoAmI];
				//Main.NewText($"Assign relevant npc data: ai0:{npc.ai[0]}, ai3:{npc.ai[3]}, style:{npc.aiStyle}");
				//Console.WriteLine($"Assign relevant npc data: ai0:{npc.ai[0]}, ai3:{npc.ai[3]}, style:{npc.aiStyle}");
				mPlayer.TargetWhoAmI = whoAmI;
				mPlayer.Player.MinionAttackTargetNPC = whoAmI;
				mPlayer.PendingTargetAssignment = -1;

				if (Main.netMode == NetmodeID.Server) {
					//Console.WriteLine($"[{Main.time}]Assigned target " + whoAmI);
				}
				else {
					//Main.NewText($"[{Main.time}]Assigned target " + whoAmI);
				}
			}
		}

		/// <summary>
		/// Called in SummonersAssociationPlayer.PostUpdate. Checks if the stored target whoAmI is still valid, and resets if not
		/// </summary>
		internal static void TargetVerification(SummonersAssociationPlayer mPlayer) {
			if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != mPlayer.Player.whoAmI) return;
			if (ServerConfig.Instance.DisableAdvancedTargetingFeature) return;
			//Only the server and the client owning it will run the code here

			int whoAmI = mPlayer.TargetWhoAmI;
			bool bad = true;

			if (whoAmI > -1 && whoAmI < Main.maxNPCs) {
				NPC npc = Main.npc[whoAmI];
				if (npc != null && npc.active && npc.type == NPCType<MinionTarget>()) {
					bad = false;
				}
			}

			if (bad && whoAmI > -1) {
				mPlayer.TargetWhoAmI = -1;

				if (Main.netMode == NetmodeID.Server) {
					//Console.WriteLine($"[{Main.time}]Detected bad " + whoAmI);
				}
				else {
					//Main.NewText($"[{Main.time}]Detected bad " + whoAmI);
				}
			}
		}
		#endregion

		//Most hooks won't be loaded if disabled in the config
		#region Hooks
		private static void ForceMinionTargetIndicatorDrawIfThisItemIsSelected(On_Main.orig_DrawInterface_1_2_DrawEntityMarkersInWorld orig) {
			var heldItem = Main.LocalPlayer.HeldItem;
			var oldDamageClass = heldItem.DamageType;
			bool reset = false;
			if (heldItem.type == ItemType<MinionControlRod>()) {
				reset = true;
				heldItem.DamageType = DamageClass.Summon; //Need to spoof the damage class so that the indicator draws. We don't want to set this permanently
			}

			orig();

			if (reset) {
				heldItem.DamageType = oldDamageClass;
			}
		}

		internal static void IgnoreTargetHealthMouseover(On_Main.orig_DrawInterface_39_MouseOver orig, Main self) {
			var resetFlag = new List<int>();
			try {
				int type = NPCType<MinionTarget>();
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc == null) {
						continue;
					}
					if (npc.active && npc.type == type && !npc.dontTakeDamage) {
						resetFlag.Add(i);
						//Vanilla code for drawing mouseover for health: if (Main.npc[num2].lifeMax > 1 && !Main.npc[num2].dontTakeDamage)
						npc.dontTakeDamage = true;
					}
				}

				//dontTakeDamage has to be false so the minions shoot at it, but for mouseover purposes, this is misleading (showing "6/6" at the end)
				orig(self);
			}
			finally {
				foreach (int index in resetFlag) {
					NPC npc = Main.npc[index];
					if (npc == null) {
						continue;
					}
					npc.dontTakeDamage = false;
				}
			}
		}

		internal static void AdjustMinionTarget(On_Player.orig_UpdateMinionTarget orig, Player self) {
			if (self.whoAmI != Main.myPlayer) {
				//Don't execute our code unnecessarily
				orig(self);
				return;
			}

			//Because we keep npc.friendly on true, there is an unintended reset happening:
			//Because this vanilla method uses CanBeChasedBy, which checks for !npc.friendly, the target gets reset,
			//here we need to revert it so the reticle is kept onto our target

			int target = self.GetModPlayer<SummonersAssociationPlayer>().TargetWhoAmI;
			int prevTarget = self.MinionAttackTargetNPC;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				//In multiplayer, the player can use his minion control rod to select another player's target
				if (prevTarget > -1) {
					NPC npc = Main.npc[prevTarget];
					if (npc != null && npc.active && npc.type == NPCType<MinionTarget>()) {
						target = prevTarget;
					}
				}
			}

			bool restore = false;
			if (target > -1 && target == prevTarget) {
				restore = true;
			}

			orig(self);

			if (restore) {
				self.MinionAttackTargetNPC = target;
				NPC npc = Main.npc[self.MinionAttackTargetNPC];
				if (!npc.active) {
					return;
				}
			}
		}

		internal static void ResetFriendlyAndChaseable(orig_ProjectileAI orig, Projectile self) {
			//Because we keep npc.friendly on true so there are no unintended interactions with the target,
			//we only set npc.friendly to false when the minions are about to do their AI

			var resetChaseable = new List<int>();
			var resetFriendly = new List<int>();
			try {
				if (self.minion || ProjectileID.Sets.MinionShot[self.type] || self.sentry || ProjectileID.Sets.SentryShot[self.type]) {
					Player player = Main.player[self.owner];
					if (player != null) {
						int type = NPCType<MinionTarget>();

						for (int j = 0; j < Main.maxNPCs; j++) {
							NPC npc = Main.npc[j];
							if (npc != null && npc.active && npc.type == type) {
								npc.friendly = false;
								resetFriendly.Add(j);

								//Setting chaseable doesn't matter in singleplayer because only 1 target exists
								if (Main.netMode == NetmodeID.SinglePlayer) continue;

								//Block minions from treating all non-targeted targets as chaseable
								//Only block chasing if the target isn't targeted (in multiplayer a player can target someone elses target)
								if (npc.chaseable && npc.whoAmI != player.MinionAttackTargetNPC/* || !mPlayer.HasTarget*/) {
									npc.chaseable = false;
									resetChaseable.Add(j);
								}
							}
						}
					}
				}

				orig(self);
			}
			finally {
				foreach (int index in resetChaseable) {
					NPC npc = Main.npc[index];
					if (npc == null) {
						continue;
					}
					npc.chaseable = true;
				}

				foreach (int index in resetFriendly) {
					NPC npc = Main.npc[index];
					if (npc == null) {
						continue;
					}
					npc.friendly = true;
				}
			}
		}

		//Reason we are doing a manual hook on ProjectileLoader.ProjectileAI instead of Projectile.AI is because for some reason the hook isn't ran
		//It registers, but breakpoints in it never get hit, yet the AI of projectiles runs fine
		internal delegate void orig_ProjectileAI(Projectile self);
		internal delegate void hook_ProjectileAI(orig_ProjectileAI orig, Projectile self);

		//namespace Terraria.ModLoader //public static class ProjectileLoader //public static void ProjectileAI(Projectile projectile)
		private static readonly MethodInfo m_ProjectileAI = typeof(ProjectileLoader).GetMethod("ProjectileAI", BindingFlags.Static | BindingFlags.Public);

		private static Hook ProjectileAIHook;

		private static bool loadedOnce = false;

		public override void Load() {
			if (loadedOnce) return; //Futureproofing in case this class gets inherited, as all the things here should only run once 
			loadedOnce = true;

			On_Main.DrawInterface_1_2_DrawEntityMarkersInWorld += ForceMinionTargetIndicatorDrawIfThisItemIsSelected;

			if (ServerConfig.Instance.DisableAdvancedTargetingFeature) return;

			On_Main.DrawInterface_39_MouseOver += IgnoreTargetHealthMouseover;

			On_Player.UpdateMinionTarget += AdjustMinionTarget;

			if (m_ProjectileAI != null) {
				try {
					ProjectileAIHook = new Hook(m_ProjectileAI, new hook_ProjectileAI(ResetFriendlyAndChaseable));
				}
				catch {
					SummonersAssociation.Instance.Logger.Error($"{nameof(ResetFriendlyAndChaseable)} failed to hook 'ProjectileAI', Minion Control Rod targeting will not work");
				}
			}
		}

		public override void Unload() {
			loadedOnce = false;
		}
		#endregion
	}
}
