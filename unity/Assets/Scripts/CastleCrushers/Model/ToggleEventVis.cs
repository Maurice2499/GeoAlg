using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CastleCrushers {

	public class ToggleEventVis : MonoBehaviour {

		CastleCrushersController controller;
		void Start() {
			controller = GetComponent<CastleCrushersController>();
		}

		void Update() {
			if (Input.GetKeyDown(KeyCode.V)) {
				controller.toggleEvents();
            }
		}
	}
}
