using UnityEngine;
using System.Collections;

public class SatteliteCamera : MonoBehaviour {
	public GameObject objectToTrack;
	public float orbitHeight = 1000;
	public bool showCube = true;

	GameObject m_cube;

	// Use this for initialization
	void Start () {
		Camera camera = GetComponent<Camera>();
		camera.farClipPlane = orbitHeight + 2000;

		if (showCube) {
			m_cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			m_cube.transform.parent = transform;
			float cubeScale = 0.02f;
			m_cube.transform.localScale = new Vector3(cubeScale, cubeScale, cubeScale);

			m_cube.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			Destroy(m_cube.GetComponent<Collider>());
		}
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = objectToTrack.transform.position;
		Vector3 center = new Vector3(pos.x, 0, pos.z);
		pos.y = orbitHeight;
		transform.position = pos;
		transform.LookAt(center);
		if (m_cube) {
			m_cube.transform.rotation = Quaternion.identity;
			m_cube.transform.position = new Vector3(center.x, transform.position.y - 1, center.z);
		}
	}
}
