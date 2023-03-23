using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurePoints : MonoBehaviour {

	[SerializeField] private Transform point1Transform;
	[SerializeField] private Transform point2Transform;
	[SerializeField] private bool bEnableMeasurement;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (bEnableMeasurement)
		{
			print("Distance: " + Vector3.Distance(point1Transform.position, point2Transform.position));
		}
	}
}
