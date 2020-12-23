using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesController : MonoBehaviour {

	[SerializeField] private GameObject wallPrefab;
	[SerializeField] private GameObject cannonPrefab;

	private Vector3 shotStart;
	private Vector3 shotEnd;
	private GameObject shot;
	private GameObject cannon;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			shotStart = Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);
			cannon = Instantiate(cannonPrefab, shotStart, Quaternion.identity);
			shot = Instantiate(wallPrefab);
			shot.GetComponent<LineRenderer>().SetPosition(0, shotStart);

        } else if (Input.GetMouseButton(0)) {
			shotEnd = shotStart + (Camera.main.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward) - shotStart).normalized * 25;
			shot.GetComponent<LineRenderer>().SetPosition(1, shotEnd);
			cannon.transform.rotation = Quaternion.LookRotation(Vector3.forward, shotEnd - cannon.transform.position);
		} else if (Input.GetMouseButtonUp(0)) {

        }
		
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
