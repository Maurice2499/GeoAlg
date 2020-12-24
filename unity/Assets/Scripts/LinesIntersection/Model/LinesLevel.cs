using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CastleCrushers {
	public class LinesLevel : MonoBehaviour {

		[Header("Level parameters")]
		public int maxCannons = 0;

		[Header("Configuration")]
		public List<LineObject> walls = new List<LineObject>();
	}
}
