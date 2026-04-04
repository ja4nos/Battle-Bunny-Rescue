namespace BBR.Events
{
	public class SavedBunniesEvent
	{
		public readonly int PlayerId;
		public readonly int SavedBunniesCount;

		public SavedBunniesEvent(int playerId, int savedBunniesCount)
		{
			PlayerId = playerId;
			SavedBunniesCount = savedBunniesCount;
		}
	}
}