using UnityEngine;
using System.Collections;

public class FirstPersonKeyboardControls : MonoBehaviour {
	public string t;
	public string c;

	void Update () {
//		if (!Input.anyKeyDown)
//			return;

		if (Input.GetMouseButtonDown(0)) {
			Vector3 worldPos;
			if (!getRayWorldPos(out worldPos))
				return;

			GameObject go = Root.instance.player.currentEntityClass.createInstance(null, "added by user");
			go.transform.position = worldPos;
			Root.instance.landscapeManager.addEntityInstance(go);
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
