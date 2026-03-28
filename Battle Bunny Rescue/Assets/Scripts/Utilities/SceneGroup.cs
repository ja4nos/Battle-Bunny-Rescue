using System.Collections.Generic;
using UnityEngine;

namespace Project.Utilities
{
	[CreateAssetMenu(menuName = "Project/Scene Group")]
	public class SceneGroup : ScriptableObject
	{
		public List<string> Scenes = new();
	}
}