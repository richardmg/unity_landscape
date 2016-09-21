using UnityEngine;
using System.Collections;

public class KeyboardControls : MonoBehaviour {
	public GameObject player;
	public string t;
	public string c;

	void Update () {
		if (!Input.anyKeyDown)
			return;

		if (Input.GetKeyDown(KeyCode.T)) {
			Vector3 worldPos;
			if (!getRayWorldPos(out worldPos))
				return;

			Thing thing = new Thing();
			thing.worldPos = worldPos;
//			thing.worldPos.y = LandscapeConstructor.instance.sampleHeight(thing.worldPos);
			thing.index = t;
			LandscapeConstructor.instance.addThing(thing);
		} else if (Input.GetKeyDown(KeyCode.C)) {
			Vector3 worldPos;
			if (!getRayWorldPos(out worldPos))
				return;

			Thing thing = new Thing();
			thing.worldPos = worldPos;
			thing.index = c;
			LandscapeConstructor.instance.addThing(thing);
		}
	}

	bool getRayWorldPos(out Vector3 worldPos)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
        if (Physics.Raycast(ray, out hit)) {
			worldPos = hit.point;
			return true;
		}
		worldPos = Vector3.zero;
		return false;
	}
}
