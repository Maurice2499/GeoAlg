using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CastleCrushers {

	public class DebugLineToggler : MonoBehaviour {

		CastleCrushersController controller;
		void Start() {
			controller = GetComponent<CastleCrushersController>();
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.I)) {
				controller.toggleEventsVis();
            }
			if (Input.GetKeyDown(KeyCode.S)) {
				controller.toggleSolutionVis();
            }
			if (Input.GetKeyDown(KeyCode.Space)) {
				controller.NextLevel();
			}
		}
	}
}
