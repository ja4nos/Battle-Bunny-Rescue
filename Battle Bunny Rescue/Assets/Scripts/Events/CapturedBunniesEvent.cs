namespace BBR.Events
{
	public class CapturedBunniesEvent
	{
		public readonly int PlayerId;
		public int CapturedBunniesCount;

		public CapturedBunniesEvent(int playerId, int capturedBunniesCount = 0)
		{
			PlayerId = playerId;
			CapturedBunniesCount = capturedBunniesCount;
		}
	}
}