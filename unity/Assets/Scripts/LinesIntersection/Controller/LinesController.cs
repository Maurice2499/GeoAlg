using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;
using UnityEngine.UI;
using CastleCrushers.Tests;
using System;

namespace CastleCrushers {
	public class LineObject {
		public LineSegment line;
		public GameObject obj;
        public Vector2 yRange; //x is lower y and y is upper y
        public bool remove = true;

		public LineObject(LineSegment line, GameObject obj) {
			this.line = line;
			this.obj = obj;
            this.yRange = new Vector2(this.Lowest().y, this.Highest().y);
		}

        public Vector2 Highest() // Assumes this line is not horizontal
        {
            return this.line.Point1.y > this.line.Point2.y ? this.line.Point1 : this.line.Point2;
        }

        public Vector2 Lowest() // Assumes this line is not horizontal
        {
            return this.line.Point1.y < this.line.Point2.y ? this.line.Point1 : this.line.Point2;
        }

        public override string ToString()
        {
            return "LO cont " + line.ToString();
        }
    }

    public class Wall
    {
        public LineObject LineObject;

        public bool broken;

        public Wall(LineObject lineObject)
        {
            LineObject = lineObject;
            broken = false;
        }

        public void Break()
        {
            this.broken = true;
        }
    }

	public class LinesController : MonoBehaviour {

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

		[SerializeField] private bool endless;

		private int maxShots = 5;

		public List<LineObject> shots = new List<LineObject>();

		public List<Wall> walls = new List<Wall>();
        
		private const float MIN_WIDTH = -7.8f;
		private const float MAX_WIDTH = 7.8f;
		private const float MIN_HEIGHT = -3.5f;
		private const float MAX_HEIGHT = 3.5f;

		private const int ENDLESS_START = 10;
		private const int ENDLESS_INCREASE = 5;
		private const int ENDLESS_MAX = 200;

		// Use this for initialization
		void Start() {
			Debug.LogWarning("Warning: maxShots is not changed based on GenerateNewLevel()!");
			Debug.LogWarning("Warning: intersections are brute-forced!");

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
			if (CheckSolution()) {
				SetWallsDestroyed(true);
				advanceButton.SetActive(true);
			}
			UpdateWallDestroyed();
			UpdateRemainingShots();
		}

		public void RemoveShot(Vector3 pos) {
			if (shots.Count == 0) {
				return;
			}

			float min = Vector3.Distance(shots[0].line.Point1, pos);
            LineObject closest = shots[0];

			for (int i = 1; i < shots.Count; i++) {
				if (Vector3.Distance(shots[i].line.Point1, pos) < min) { // Alternatively, closest line: shots[i].line.DistanceToPoint(pos) < min
					closest = shots[i];
					min = shots[i].line.DistanceToPoint(pos);
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
			foreach (Wall wall in walls) {
				Destroy(wall.LineObject.obj);
			}
			walls = new List<Wall>();
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

				walls.Add(new Wall(new LineObject(new LineSegment(level.startPoints[i], level.endPoints[i]), newWall)));
            }
		}

		public void GenerateNewLevel(int maxWalls) {
			ClearLevel();

            Debug.LogWarning("Running tests");
            new SweepTest();
            Debug.LogWarning("Tests completed");

            // Run it 10 times because it is randomized. I dont expect to catch all bugs but at least this is better than only once
            maxWalls = 10;
            bool horizontalLine = true;
            while (horizontalLine) // change this number for more random!
            {
                horizontalLine = false;
                // Generator code
                List<LineObject> lines = new List<LineObject>();
                foreach (Wall wall in walls)
                {
                    Destroy(wall.LineObject.obj);
                }
                walls = new List<Wall>();
                for (int i = 0; i < maxWalls; i++)
                {
                    Vector2 position1 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));
                    Vector2 position2 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));
                   

                    GameObject newWall = Instantiate(wallPrefab, wallObjects);
                    newWall.transform.Find("StartCastle").position = position1;
                    newWall.transform.Find("EndCastle").position = position2;
                    newWall.GetComponent<LineRenderer>().SetPosition(0, position1);
                    newWall.GetComponent<LineRenderer>().SetPosition(1, position2);


                    LineSegment newLine = new LineSegment(position1, position2);
                    LineObject newLineObj = new LineObject(newLine, newWall);
                    lines.Add(newLineObj);
                    walls.Add(new Wall(newLineObj));

                    if (newLine.IsHorizontal) {
                        horizontalLine = true;
                        break;
                    }
                }
                
                if (horizontalLine)
                {
                    continue;
                }

                DownwardSweepLine sweep = new DownwardSweepLine(lines);
                List<Intersection> intersections = sweep.Run();
                foreach (Intersection intersection in intersections)
                {
                    intersection.two.remove = true;
                }
                lines.RemoveAll(item => item.remove);

                DownwardSweepLine sweep2 = new DownwardSweepLine(lines);
                if (sweep2.Run().Count != 0)
                {
                    throw new Exception("Test error");
                }
            }
		}

		private void SetWallsDestroyed(bool destroyed) {
			Sprite sprite;
			Material mat;
			if (destroyed) {
				sprite = castleDestroyedSprite;
				mat = wallDestroyedMat;
			} else {
				sprite = castleSprite;
				mat = wallMat;
			}

			foreach (Wall wall in walls) {
				wall.LineObject.obj.GetComponent<Renderer>().material = mat;
				wall.LineObject.obj.transform.Find("StartCastle").GetComponent<SpriteRenderer>().sprite = sprite;
				wall.LineObject.obj.transform.Find("EndCastle").GetComponent<SpriteRenderer>().sprite = sprite;
			}
		}

		// Updates textures of walls according to whether they have been shot or not.
		private void UpdateWallDestroyed()
        {
            foreach (Wall wall in walls)
			{
				if (wall.broken) {
					wall.LineObject.obj.GetComponent<Renderer>().material = wallDestroyedMat;
				} else {
					wall.LineObject.obj.GetComponent<Renderer>().material = wallMat;
				}
			}
		}

		//To check the solution
		public bool CheckSolution() {
			foreach (Wall wall in walls) {
				if (!wall.broken) {
					return false;
				}
			}
			return true;
		}   

		public void UpdateRemainingShots() {
			remaining.text = "Shots: " + shots.Count + "/" + maxShots;
		}
	}
}