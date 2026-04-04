using System;
using UnityEngine;

namespace Pool.Pool
{
	[Serializable]
	public class ParticlePool : PoolBase<ParticleSystem>
	{
		public override void Return(ParticleSystem item)
		{
			item.Stop();
			item.gameObject.SetActive(false);
		}
	}
}
