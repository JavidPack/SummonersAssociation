namespace SummonersAssociation
{
	public enum PacketType : byte
	{
		None = 0,
		/// <summary>
		/// Sent from a client to the server when he wants to spawn a target. Server then spawns it and sends SpawnTarget back
		/// </summary>
		SpawnTarget = 1,
		/// <summary>
		/// Server sends the players target whoAmI, which is then set to pending
		/// </summary>
		ConfirmTargetToClient = 2,
		/// <summary>
		/// Syncs relevant player data on join
		/// </summary>
		SyncPlayer = 3,
	}
}
