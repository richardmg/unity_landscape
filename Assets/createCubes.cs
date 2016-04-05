using UnityEngine;
using System.Collections;

public class createCubes : MonoBehaviour {

	// Use this for initialization
	void Start () {
		print ("Starting to create cubes");
		GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
		cube.transform.position = new Vector3 (0, 0, -7);

	}
	
	// Update is called once per frame
	void Update () {
	}
}
