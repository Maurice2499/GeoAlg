using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;
using UnityEngine.UI;
using CastleCrushers.Tests;
using System;
using System.Linq;

namespace CastleCrushers {
	public class CastleCrushersController : MonoBehaviour {

		[SerializeField] private List<LinesLevel> levels;
		private int level = 0;

		[SerializeField] private GameObject wallPrefab;
		[SerializeField] private Transform wallObjects;
		[SerializeField] private int maxWalls;

		[SerializeField] private Sprite castleSprite;
		[SerializeField] private Sprite castleDestroyedSprite;
		[SerializeField] private Material wallMat;
		[SerializeField] private Material wallDestroyedMat;
		[SerializeField] private Text remaining;

		[SerializeField] private GameObject advanceButton;

		[SerializeField] private Transform eventObjects;
		[SerializeField] private Transform solverShotObjects;

		[SerializeField] private bool endless;

		private const float MIN_WALL_SIZE = 2;

		private int maxShots = 5;

		public List<LineObject> shots = new List<LineObject>();

		public List<LineObject> walls = new List<LineObject>();

		private const float MIN_WIDTH = -7.8f;
		private const float MAX_WIDTH = 7.8f;
		private const float MIN_HEIGHT = -3.5f;
		private const float MAX_HEIGHT = 3.5f;

		private const int ENDLESS_START = 3;
		private const int ENDLESS_INCREASE = 1;

		private const int ENDLESS_MAX = 25;

		private DownwardSweepLine sweep;
		private ShotSolver shotSolver;

		// Use this for initialization
		void Start() {
			// run tests
			SweepTest test = new SweepTest();

			if (endless) {
				GenerateNewLevel(ENDLESS_START);
			} else {
				LoadLevel(0);
			}

			UpdateRemainingShots();
		}

		public bool CanAddShot() {
			return shots.Count < maxShots;
		}

		public bool IsLevelComplete() {
			return advanceButton.activeSelf;
		}

		public void AddNewShot(LineObject shot) {
			shots.Add(shot);
			foreach (LineObject wall in walls) {
				Vector2? intersection = wall.Intersect(shot);
				if (intersection != null) {
					wall.Break();
				}
			}

			if (CheckSolution()) {
				advanceButton.SetActive(true);
			}

			UpdateWallDestroyed();
			UpdateRemainingShots();
		}

		public void RemoveShot(Vector3 pos) {
			if (shots.Count == 0) {
				return;
			}

			float min = Vector3.Distance(shots[0].Point1, pos);
			LineObject closest = shots[0];

			for (int i = 1; i < shots.Count; i++) {
				if (Vector3.Distance(shots[i].Point1, pos) < min) { // Alternatively, closest line: shots[i].line.DistanceToPoint(pos) < min
					closest = shots[i];
					min = shots[i].DistanceToPoint(pos);
				}
			}

			foreach (LineObject wall in walls) {
				Vector2? intersection = wall.Intersect(closest);
				if (intersection != null) {
					wall.repair();
				}
			}

			shots.Remove(closest);
			Destroy(closest.obj);
			UpdateWallDestroyed();
			UpdateRemainingShots();
		}

		public void NextLevel() {
			advanceButton.SetActive(false);
			level++;

			if (endless) {
				// endless mode
				GenerateNewLevel(Mathf.Min(ENDLESS_START + level * ENDLESS_INCREASE, ENDLESS_MAX));
			} else if (level >= levels.Count) {
				// not endless mode and all levels completed
				UnityEngine.SceneManagement.SceneManager.LoadScene("linesVictory");
			} else {
				// not endless mode and not all levels completed
				LoadLevel(level);
			}

			UpdateRemainingShots();
		}

		private void ClearLevel() {
			foreach (LineObject shot in shots) {
				Destroy(shot.obj);
			}
			shots = new List<LineObject>();
			foreach (LineObject wall in walls) {
				Destroy(wall.obj);
			}
			walls = new List<LineObject>();
		}

		public void toggleEventsVis() {
			eventObjects.gameObject.SetActive(!eventObjects.gameObject.activeSelf);
        }

		private void updateEventsVis(List<LineObject> eventWalls, List<SweepEvent> eventPoints) {
			if (sweep == null) {
				return;
            }

			// destroy all events lines
			foreach (Transform child in eventObjects) {
				Destroy(child.gameObject);
            }

			foreach (SweepEvent ev in eventPoints) {
				Color color = Color.green; // Default: insert
				if (ev.EventType == EventType.DELETE) {
					color = Color.red;
				} else if (ev.EventType == EventType.INTERSECT) {
					color = Color.blue;
                }
				drawLine(new Vector3(ev.Pos.x - 0.1f, ev.Pos.y, -3),
					new Vector3(ev.Pos.x + 0.1f, ev.Pos.y, -3),
					0.2f,
					color,
					eventObjects);
			}

			foreach (LineObject wall in eventWalls) {
				drawLine(new Vector3(wall.Point1.x, wall.Point1.y, -2),
					new Vector3(wall.Point2.x, wall.Point2.y, -2),
					0.05f,
					Color.yellow,
					eventObjects);
			}
        }

		public void toggleSolutionVis() {
			solverShotObjects.gameObject.SetActive(!solverShotObjects.gameObject.activeSelf);
		}
		private void updateSolverVis(List<Line> lines) {
			foreach (Transform shot in solverShotObjects) {
				Destroy(shot.gameObject);
			}
			foreach (Line line in lines) {
				Vector2 dir = (line.Point2 - line.Point1).normalized;
				Vector3 start = line.Point1 - 30 * dir;
				Vector3 end = line.Point1 + 30 * dir;
				start.z = -1;
				end.z = -1;
				drawLine(start, end, 0.1f, Color.magenta, solverShotObjects);
            }
        }

		private void drawLine(Vector3 start, Vector3 end, float width, Color color, Transform parent) {
			GameObject line = new GameObject("event", typeof(LineRenderer));
			line.transform.parent = parent;
			LineRenderer renderer = line.GetComponent<LineRenderer>();
			renderer.startWidth = width;
			renderer.endWidth = width;
			renderer.SetPosition(0, start);
			renderer.SetPosition(1, end);
			renderer.material = new Material(Shader.Find("Unlit/Color"));
			renderer.material.color = color;
		}

		private void LoadLevel(int id) {
			ClearLevel();

			LinesLevel level = levels[id];
			maxShots = level.maxShots;
			for (int i = 0; i < level.startPoints.Count; i++) {
				GameObject newWall = Instantiate(wallPrefab, wallObjects);
				newWall.transform.Find("StartCastle").position = level.startPoints[i];
				newWall.transform.Find("EndCastle").position = level.endPoints[i];
				newWall.GetComponent<LineRenderer>().SetPosition(0, level.startPoints[i]);
				newWall.GetComponent<LineRenderer>().SetPosition(1, level.endPoints[i]);

				walls.Add(new LineObject(level.startPoints[i], level.endPoints[i], newWall));
			}
		}

		public void GenerateNewLevel(int maxWalls) {
			ClearLevel();
            
            // Generator code
            walls = new List<LineObject>();
			List<LineObject> eventWalls = new List<LineObject>();

			int N = maxWalls;
			while (N > 0) {
				Vector2 position1 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));
				Vector2 position2 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));

				LineObject newLine = new LineObject(position1, position2);

				if (newLine.IsHorizontal == false) {
					walls.Add(newLine);
					N -= 1;
				}
			}

			// Remove all too short walls
			walls.RemoveAll(item => item.Magnitude < MIN_WALL_SIZE);
			
			foreach (LineObject wall in walls) {
				eventWalls.Add(new LineObject(new Vector2(wall.Point1.x, wall.Point1.y), new Vector2(wall.Point2.x, wall.Point2.y))); // deep copy
            }

			sweep = new DownwardSweepLine(walls);
			List<Intersection> intersections = sweep.Run();

			List<SweepEvent> eventPoints = new List<SweepEvent>();
			foreach (SweepEvent ev in sweep.producedEvents) {
				SweepEvent copy = new SweepEvent(ev.EventType); // deep copy
				copy.StatusItem = new StatusItem(new LineObject(ev.StatusItem.LineObject.Point1, ev.StatusItem.LineObject.Point2));
				if (ev.IntersectingStatusItem != null) {
					copy.IntersectingStatusItem = new StatusItem(new LineObject(ev.IntersectingStatusItem.LineObject.Point1, ev.IntersectingStatusItem.LineObject.Point2));
				}
				eventPoints.Add(copy);
            }

			// Split intersections into multiple walls doesnt always work. If there are many walls we cant just simply split
			Dictionary<LineObject, List<Vector2>> map = new Dictionary<LineObject, List<Vector2>>();
			for (int i = 0; i < intersections.Count; i++) {
				if (!map.ContainsKey(intersections[i].one)) {
					map.Add(intersections[i].one, new List<Vector2>());
				}
				if (!map.ContainsKey(intersections[i].two)) {
					map.Add(intersections[i].two, new List<Vector2>());
				}
				Vector2? intersection = intersections[i].one.Intersect(intersections[i].two);
				// safety check
				if (intersection != null) {
					map[intersections[i].one].Add((Vector2)intersection);
					map[intersections[i].two].Add((Vector2)intersection);
				}
			}

			foreach (LineObject oldSeg in map.Keys) {
				List<Vector2> points = map[oldSeg];
				points.Add(oldSeg.Point1);
				points.Add(oldSeg.Point2);
				points.Sort((a, b) => a.x.CompareTo(b.x));
				walls.Remove(oldSeg);
				for (int i = 0; i < points.Count - 1; i++) {
					walls.Add(new LineObject(points[i], points[i + 1]));
				}
			}

			//walls.RemoveAll(item => item.hits > 0);

			// Remove all too short walls
			walls.RemoveAll(item => item.Magnitude < MIN_WALL_SIZE);


			// Add GameObject to the remaining walls
			float sizeFactor = 0.8f * (float)Math.Exp(-0.03 * ((float)walls.Count)) + 0.01f;
			foreach (LineObject line in walls) {
				Vector2 position1 = line.Point1;
				Vector2 position2 = line.Point2;

				GameObject newWall = Instantiate(wallPrefab, wallObjects);
				newWall.transform.Find("StartCastle").position = position1;
				newWall.transform.Find("EndCastle").position = position2;
				newWall.GetComponent<LineRenderer>().SetPosition(0, position1);
				newWall.GetComponent<LineRenderer>().SetPosition(1, position2);

				newWall.transform.Find("StartCastle").localScale = new Vector3(sizeFactor, sizeFactor, sizeFactor);
				newWall.transform.Find("EndCastle").localScale = new Vector3(sizeFactor, sizeFactor, sizeFactor);
				newWall.GetComponent<LineRenderer>().startWidth = 0.4f * sizeFactor;
				newWall.GetComponent<LineRenderer>().endWidth = 0.4f * sizeFactor;

				line.obj = newWall;
			}

			// Use ShotSolver to find the budget of shots
			shotSolver = new ShotSolver(walls);

			List<Line> greedyShots = shotSolver.GreedyCover();
			maxShots = greedyShots.Count;
			updateEventsVis(eventWalls, eventPoints);
			updateSolverVis(greedyShots);
		}

		// Updates textures of walls according to whether they have been shot or not.
		private void UpdateWallDestroyed() {
			foreach (LineObject wall in walls) {
				if (wall.hits > 0) {
					wall.obj.GetComponent<Renderer>().material = wallDestroyedMat;
					wall.obj.transform.Find("StartCastle").GetComponent<SpriteRenderer>().sprite = castleDestroyedSprite;
					wall.obj.transform.Find("EndCastle").GetComponent<SpriteRenderer>().sprite = castleDestroyedSprite;
				} else {
					wall.obj.GetComponent<Renderer>().material = wallMat;
					wall.obj.transform.Find("StartCastle").GetComponent<SpriteRenderer>().sprite = castleSprite;
					wall.obj.transform.Find("EndCastle").GetComponent<SpriteRenderer>().sprite = castleSprite;
				}
			}
		}

		//To check the solution
		public bool CheckSolution() {
			foreach (LineObject wall in walls) {
				if (!(wall.hits > 0)) {
					return false;
				}
			}
			return true;
		}

		public void UpdateRemainingShots() {
			remaining.text = "Shots: " + shots.Count + "/" + maxShots;
		}
	}

	public class LineObject : LineSegment {
		public GameObject obj;
		public int hits = 0;

		public LineObject(Vector2 point1, Vector2 point2, GameObject obj = null) : base(point1, point2) {
			this.obj = obj;
		}

		// Changes the highest point.
		public void NewHighest(Vector2 newTop) {
			Point1 = newTop;
			Point2 = Lowest();

			// explicitly calculate variables that are most used
			XInterval = new FloatInterval(Point1.x, Point2.x);
			YInterval = new FloatInterval(Point1.y, Point2.y);
			Line = new Line(Point1, Point2);
		}

		public Vector2 Highest() // Assumes this line is not horizontal
		{
			return Point1.y > Point2.y ? Point1 : Point2;
		}

		public Vector2 Lowest() // Assumes this line is not horizontal
		{
			return Point1.y < Point2.y ? Point1 : Point2;
		}

		public void Break() {
			hits++;
		}

		public void repair() {
			if (hits > 0) {
				hits--;
			}
		}

		public override string ToString() {
			return "LO cont " + ToString();
		}
	}
}