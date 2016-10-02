using UnityEngine;
using System.Collections;

public class FirstPersonKeyboardControls : MonoBehaviour {
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
		} else if (Input.GetKeyDown(KeyCode.X)) {
			//			CharacterController controller = player.GetComponent<CharacterController>();
			//			Vector3 worldPos = player.transform.position;
			//			worldPos.y = LandscapeConstructor.instance.sampleHeight(worldPos) + 1;
			//			player.transform.position = worldPos;
		}
	}

	bool getRayWorldPos(out Vector3 worldPos)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
		//		Debug.DrawRay(ray.origin, ray.direction, Color.red, 5f);
		if (Physics.Raycast(ray, out hit)) {
			worldPos = hit.point;
			return true;
		}
		worldPos = Vector3.zero;
		return false;
	}
}
