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
			GameObject gameObject;
			if (!getRayWorldPos(out worldPos, out gameObject))
				return;

			EntityClass entityClass = Root.instance.player.currentEntityClass();
			if (entityClass == null)
				return;

			if (Input.GetKey(KeyCode.LeftShift)) {
				EntityInstance entityInstance = gameObject.GetComponent<EntityInstance>();
				if (entityInstance) {
					UIManager uiMgr = Root.instance.uiManager;
					uiMgr.entityClassPicker.selectEntityClass(entityInstance.entityClass);
					uiMgr.entityPainter.setEntityInstance(entityInstance);
					uiMgr.uiPaintEditorGO.pushDialog();
				}
			} else {
				EntityInstance entityInstance = entityClass.createInstance(null, "added by user");
				entityInstance.gameObject.transform.position = worldPos;
				Root.instance.landscapeManager.addEntityInstance(entityInstance);
			}
		}
	}

	bool getRayWorldPos(out Vector3 worldPos, out GameObject gameObject)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
		//		Debug.DrawRay(ray.origin, ray.direction, Color.red, 5f);
		if (Physics.Raycast(ray, out hit)) {
			worldPos = hit.point;
			gameObject = hit.transform.gameObject;
			return true;
		}
		worldPos = Vector3.zero;
		gameObject = null;
		return false;
	}
}
