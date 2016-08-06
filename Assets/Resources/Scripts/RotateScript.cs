using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {
	public bool rotate = false;

	// Use this for initialization
	void Start () {
	
	}
	
	public void Update()
	{
		if (rotate)
			transform.Rotate(new Vector3(0, 0.2f, 0));
	}
}
