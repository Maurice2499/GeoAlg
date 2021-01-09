using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;
using UnityEngine.UI;
using CastleCrushers.Tests;
using System;
using System.Linq;

namespace CastleCrushers {
	public class LineObject {
		public LineSegment line;
		public GameObject obj;
        public Vector2 yRange; //x is lower y and y is upper y
		public int hits = 0;

		public LineObject(LineSegment line, GameObject obj) {
			this.line = line;
			this.obj = obj;
            this.yRange = new Vector2(this.Lowest().y, this.Highest().y);
		}

		// Changes the highest point.
		public void NewHighest(Vector2 newTop)
        {
			this.line = new LineSegment(newTop, this.Lowest());
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
 
        public void Break()
        {
			this.hits++;
        }

		public void repair()
		{
			if (hits > 0)
            {
				this.hits--;
			}
		}

		public override string ToString()
        {
            return "LO cont " + line.ToString();
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
		private const int ENDLESS_MAX = 200;

        // Use this for initialization
        void Start() {

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
			foreach (LineObject wall in walls)
			{
				Vector2? intersection = wall.line.Intersect(shot.line);
				if (intersection != null)
				{
					wall.Break();
				}
			}

			if (CheckSolution())
			{
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

			foreach (LineObject wall in walls)
			{
				Vector2? intersection = wall.line.Intersect(closest.line);
				if (intersection != null)
				{
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

		private void LoadLevel(int id) {
			ClearLevel();

			LinesLevel level = levels[id];
			maxShots = level.maxShots;
			for (int i = 0; i < level.startPoints.Count; i++) {
				GameObject newWall = Instantiate(wallPrefab, null);
				newWall.transform.Find("StartCastle").position = level.startPoints[i];
				newWall.transform.Find("EndCastle").position = level.endPoints[i];
				newWall.GetComponent<LineRenderer>().SetPosition(0, level.startPoints[i]);
				newWall.GetComponent<LineRenderer>().SetPosition(1, level.endPoints[i]);

				walls.Add(new LineObject(new LineSegment(level.startPoints[i], level.endPoints[i]), newWall));
            }
		}

		public void GenerateNewLevel(int maxWalls) {
			ClearLevel();
           
			// Generator code
			walls = new List<LineObject>();

            int N = maxWalls;
            while (N > 0)
            {
                Vector2 position1 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));
                Vector2 position2 = new Vector3(UnityEngine.Random.Range(MIN_WIDTH, MAX_WIDTH), UnityEngine.Random.Range(MIN_HEIGHT, MAX_HEIGHT));

                LineSegment newLine = new LineSegment(position1, position2);

                if (newLine.IsHorizontal == false)
                {
                    walls.Add(new LineObject(newLine, null));
                    N -= 1;
                }
            }

            // Remove all too short walls
            walls.RemoveAll(item => item.line.Magnitude < MIN_WALL_SIZE);

            DownwardSweepLine sweep = new DownwardSweepLine(walls);
            List<Intersection> intersections = sweep.Run();

            // Split intersections into multiple walls doesnt always work. If there are many walls we cant just simply split
            foreach (Intersection intersection in intersections)
            {
                if (intersection.one.hits > 0 || intersection.two.hits > 0)
                {
                    continue;
                }
                else if (intersection.one.line.Magnitude < 2 * MIN_WALL_SIZE || intersection.two.line.Magnitude < 2 * MIN_WALL_SIZE)
                {
                    if (intersection.one.line.Magnitude < 2 * MIN_WALL_SIZE)
                    {
                        intersection.one.hits += 1;
                    }
                    else
                    {
                        intersection.two.hits += 1;
                    }
                }
                else
                {
                    Vector2 point = (Vector2)intersection.two.line.Intersect(intersection.one.line);
                    LineSegment oneTophalf = new LineSegment(intersection.one.Highest(), point);
                    if (oneTophalf.Magnitude > MIN_WALL_SIZE)
                    {
                        LineObject wallOne = new LineObject(oneTophalf, null);
                        walls.Add(wallOne);
                    }

                    LineSegment twoTophalf = new LineSegment(intersection.two.Highest(), point);
                    if (twoTophalf.Magnitude > MIN_WALL_SIZE)
                    {
                        LineObject wallTwo = new LineObject(twoTophalf, null);
                        walls.Add(wallTwo);
                    }

                    intersection.one.NewHighest(point);
                    intersection.two.NewHighest(point);

                    if (intersection.one.line.Magnitude < MIN_WALL_SIZE)
                    {
                        intersection.one.hits += 1;
                    }
                    if (intersection.two.line.Magnitude < MIN_WALL_SIZE)
                    {
                        intersection.two.hits += 1;
                    }
                }
			}

            walls.RemoveAll(item => item.hits > 0);

            // Remove all too short walls
            walls.RemoveAll(item => item.line.Magnitude < MIN_WALL_SIZE);


            // Add GameObject to the remaining walls
            float sizeFactor = 0.8f * (float)Math.Exp(-0.03 * ((float)walls.Count)) + 0.01f;
            foreach (LineObject line in walls)
            {
                Vector2 position1 = line.line.Point1;
                Vector2 position2 = line.line.Point2;

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
            ShotSolver shotSolver = new ShotSolver(walls);

            List<Line> greedyShots = shotSolver.GreedyCover();
            maxShots = greedyShots.Count;
		}

				// Updates textures of walls according to whether they have been shot or not.
		private void UpdateWallDestroyed()
        {
            foreach (LineObject wall in walls)
			{
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
}