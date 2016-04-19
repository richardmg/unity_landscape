using UnityEngine;
using System.Collections;

public class SatteliteCamera : MonoBehaviour {
	public GameObject objectToTrack;
	public float orbitHeight;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = objectToTrack.transform.position;
		pos.y = orbitHeight;
		transform.position = pos;
		transform.LookAt(objectToTrack.transform);
	}
}
