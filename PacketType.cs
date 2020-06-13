namespace SummonersAssociation
{
	public enum PacketType : byte
	{
		None = 0,
		/// <summary>
		/// Sent from a client to the server when he wants to spawn a targeting dummy. Server then spawns it and sends SpawnTarget back
		/// </summary>
		SpawnTarget = 1,
		/// <summary>
		/// Server sends the players target whoAmI, which is then set to pending
		/// </summary>
		ConfirmTargetToClient = 2
	}
}
