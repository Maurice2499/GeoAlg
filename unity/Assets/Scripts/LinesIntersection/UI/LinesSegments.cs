using System.Collections;
using System.Collections.Generic;
using Util.Geometry;
using UnityEngine;

namespace CastleCrushers {
	public class LinesSegments : MonoBehaviour {

		[SerializeField] private LinesController controller;

		[SerializeField] private GameObject shotLinePrefab;
		[SerializeField] private GameObject intPrefab;
		[SerializeField] private Transform intObjs;

		[SerializeField] private bool drawSkulls = false;

		private Vector3 shotStart;
		private Vector3 shotEnd;
		private GameObject shot;

		void Update() {
			if (controller.IsLevelComplete()) {
				// Do nothing when level is complete
				return;
			}

			if (Input.GetMouseButtonDown(0) && controller.CanAddShot()) {
				CreateNewShot();
			} else if (Input.GetMouseButton(0) && controller.CanAddShot()) {
				UpdateNewShotEndpoint();
			} else if (Input.GetMouseButtonUp(0) && controller.CanAddShot() && shot != null) {
				if (shotEnd.Equals(shotStart)) {
					Destroy(shot);
				} else {
					LineSegment line = new LineSegment(shotStart, shotEnd);
					controller.AddNewShot(new LineObject(line, shot));
				}
				shot = null;
			} else if (Input.GetMouseButtonDown(1)) {
				controller.RemoveShot(Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward));
			}
			if (Input.GetKeyDown(KeyCode.Space)) {
				controller.NextLevel();
			}
		}

		void LateUpdate() {
			List<Vector2> intersections = controller.IsLevelComplete() || !drawSkulls ? new List<Vector2>() : ComputeIntersections(controller.shots, controller.walls);
			IncreaseIntersections(intersections.Count);

			for (int i = 0; i < intersections.Count; i++) {
				intObjs.GetChild(i).position = intersections[i];
			}

			for (int i = intersections.Count; i < intObjs.childCount; i++) {
				intObjs.GetChild(i).position = new Vector3(-20, 0, 0);
			}
		}

		public void CreateNewShot() {
			shotStart = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
			shot = Instantiate(shotLinePrefab, shotStart, Quaternion.identity, transform);
			shot.GetComponent<LineRenderer>().SetPosition(0, shotStart);
		}

		public void UpdateNewShotEndpoint() {
			shotEnd = shotStart + (Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward) - shotStart).normalized * 25;
			shot.GetComponent<LineRenderer>().SetPosition(1, shotEnd);
			Transform cannon = shot.transform.Find("Cannon");
			cannon.rotation = Quaternion.LookRotation(Vector3.forward, shotEnd - cannon.position);
		}

		private List<Vector2> ComputeIntersections(List<LineObject> shots, List<LineObject> walls) {
			List<Vector2> result = new List<Vector2>();

			if (shot != null) {
				foreach (LineObject wall in walls) {
					Vector2? intersection = wall.line.Intersect(new LineSegment(shotStart, shotEnd));
					if (intersection != null) {
						result.Add((Vector2)intersection);
					}
				}
			}

			return result;
		}

		private void IncreaseIntersections(int amount) {
			for (int i = intObjs.childCount; i < amount; i++) {
				Instantiate(intPrefab, intObjs);
			}
		}
	}
}
