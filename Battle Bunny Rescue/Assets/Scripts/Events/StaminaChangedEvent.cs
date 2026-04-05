namespace BBR.Events
{
	public class StaminaChangedEvent
	{
		public readonly int PlayerId;
		public float StaminaPercentage;

		public StaminaChangedEvent(int playerId, float staminaPercentage = 0)
		{
			PlayerId = playerId;
			StaminaPercentage = staminaPercentage;
		}
	}
}