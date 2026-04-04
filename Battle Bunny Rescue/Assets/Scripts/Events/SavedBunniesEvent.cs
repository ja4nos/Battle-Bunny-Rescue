namespace BBR.Events
{
	public class SavedBunniesEvent
	{
		public readonly int PlayerId;
		public int SavedBunniesCount;

		public SavedBunniesEvent(int playerId, int savedBunniesCount = 0)
		{
			PlayerId = playerId;
			SavedBunniesCount = savedBunniesCount;
		}
	}
}