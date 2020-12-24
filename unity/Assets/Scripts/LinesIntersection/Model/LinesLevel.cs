using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

namespace CastleCrushers {

	[CreateAssetMenu(fileName = "ccLevelNew", menuName = "Levels/Castle Crushers Level")]
	public class LinesLevel : ScriptableObject {

		[Header("Level parameters")]
		public int maxShots = 0;

		[Header("Walls")]
		public List<Vector2> startPoints = new List<Vector2>();
		public List<Vector2> endPoints = new List<Vector2>();
	}
}
