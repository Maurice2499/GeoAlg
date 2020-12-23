using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry;

public class LinesController : MonoBehaviour {

	[SerializeField] private GameObject wallPrefab;
	[SerializeField] private GameObject shotLinePrefab;
	[SerializeField] private Transform shotObjects;

	private Vector3 shotStart;
	private Vector3 shotEnd;
	private GameObject shot;
	private HashSet<GameObject> shots = new HashSet<GameObject>();

    private List<LineSegment> wallCoordinates = new List<LineSegment>();
    private List<GameObject> wallObjects = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 screenPosition1 = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0));
            Vector3 screenPosition2 = Camera.main.ScreenToWorldPoint(new Vector3(Random.Range(0, Screen.width), Random.Range(0, Screen.height), 0));
            screenPosition1.z = 0;
            screenPosition2.z = 0;
            Vector2 projectedPosition1 = new Vector2(screenPosition1.x, screenPosition1.y);
            Vector2 projectedPosition2 = new Vector2(screenPosition2.x, screenPosition2.y);
            LineSegment line = new LineSegment(projectedPosition1, projectedPosition2);
            wallCoordinates.Add(line);

            GameObject wall = Instantiate(wallPrefab);
            wall.GetComponent<LineRenderer>().SetPosition(0, screenPosition1);
            wall.GetComponent<LineRenderer>().SetPosition(1, screenPosition2);
            wallObjects.Add(wall);
        }
    }

    // Update is called once per frame
    void Update () {
		if (Input.GetMouseButtonDown(0)) {
			CreateNewShot();
        } else if (Input.GetMouseButton(0)) {
			UpdateNewShotEndpoint();
		} else if (Input.GetMouseButtonUp(0)) {
			AddNewShot();
        }
	}

	public void CreateNewShot() {
		shotStart = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
		shot = Instantiate(shotLinePrefab, shotStart, Quaternion.identity, shotObjects);
		shot.GetComponent<LineRenderer>().SetPosition(0, shotStart);
	}

	public void UpdateNewShotEndpoint() {
		shotEnd = shotStart + (Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward) - shotStart).normalized * 25;
		shot.GetComponent<LineRenderer>().SetPosition(1, shotEnd);
		Transform cannon = shot.transform.Find("Cannon");
		cannon.rotation = Quaternion.LookRotation(Vector3.forward, shotEnd - cannon.position);
	}

	public void AddNewShot() {
		if (shotEnd.Equals(shotStart)) {
			Destroy(shot);
        } else {
			shots.Add(shot);
        }
		shot = null;
    }

	// To draw a line segment.
	public void AddSegment() {

	}

	//To move a drawn line segment
	public void MoveSegment() {

	}

	//To check the solution
	public void CheckSolution(){
		
	}
}
