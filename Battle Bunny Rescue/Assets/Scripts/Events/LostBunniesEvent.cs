using System.Collections.Generic;
using UnityEngine;

namespace BBR.Events
{
	public class LostBunniesEvent
	{
		public readonly IReadOnlyList<GameObject> LostBunnies;

		public LostBunniesEvent(IReadOnlyList<GameObject> lostBunnies)
		{
			LostBunnies = lostBunnies;
		}
	}
}
