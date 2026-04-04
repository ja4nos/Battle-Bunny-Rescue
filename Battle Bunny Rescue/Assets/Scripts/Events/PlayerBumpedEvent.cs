namespace BBR.Events
{
	public class PlayerBumpedEvent
	{
		public readonly int PlayerId;

		public PlayerBumpedEvent(int playerId)
		{
			PlayerId = playerId;
		}
	}
}
