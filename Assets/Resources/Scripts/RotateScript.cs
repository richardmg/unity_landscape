using UnityEngine;
using System.Collections;

public class RotateScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	public void Update()
	{
		transform.Rotate(new Vector3(0, 0.2f, 0));
	}
}
