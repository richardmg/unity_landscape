using UnityEngine;
using System.Collections;

public class SatteliteCamera : MonoBehaviour {
	public GameObject objectToTrack;
	public float orbitHeight = 1000;
	public bool showCube = true;

	// Use this for initialization
	void Start () {
		if (showCube) {
			float cubeScale = orbitHeight / 100f;
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			cube.transform.localScale = new Vector3(cubeScale, cubeScale, cubeScale);
			cube.transform.parent = objectToTrack.transform;
			cube.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

			GetComponent<Camera>().farClipPlane = orbitHeight + 2000;
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = objectToTrack.transform.position;
		pos.y = orbitHeight;
		transform.position = pos;
		transform.LookAt(new Vector3(pos.x, 0, pos.z));
	}
}
