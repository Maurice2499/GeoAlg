using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinesController : MonoBehaviour {

	[SerializeField] private GameObject wallPrefab;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			GameObject wall = Instantiate(wallPrefab);
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
