using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {
	public float speed = 0.1f;
	public bool rotate = false;

	// Use this for initialization
	void Start () {
	
	}
	
	public void Update()
	{
		if (rotate)
			transform.Rotate(new Vector3(0, speed, 0));
	}
}
