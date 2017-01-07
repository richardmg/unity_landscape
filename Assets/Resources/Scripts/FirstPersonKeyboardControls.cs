using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FirstPersonKeyboardControls : MonoBehaviour
{
	GameObject m_activeEntity;
	Vector3 m_offset;
	Vector3 m_moveAxis = new Vector3();

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			m_offset = m_activeEntity.transform.position - Camera.main.transform.position;
			m_moveAxis.x = m_moveAxis.x == 0 ? 1 : 0;
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			m_offset = m_activeEntity.transform.position - Camera.main.transform.position;
			m_moveAxis.y = m_moveAxis.y == 0 ? 1 : 0;
			return;
		}

		if (Input.GetKeyDown(KeyCode.Alpha3)) {
			m_offset = m_activeEntity.transform.position - Camera.main.transform.position;
			m_moveAxis.z = m_moveAxis.z == 0 ? 1 : 0;
			return;
		}
			
		if (Input.GetMouseButtonDown(0)) {
			Vector3 pos;
			if (m_activeEntity) {
				pos = m_activeEntity.transform.position;
			} else {
				pos = Camera.main.transform.position + (Camera.main.transform.forward * 5);
				pos.y = Root.instance.landscapeManager.sampleHeight(pos);
			}

			VoxelObject vo = new VoxelObject(0, 4);
			m_activeEntity = vo.createGameObject(null, Root.kLod0, true);
			m_activeEntity.transform.position = pos;
		}	

		if (m_activeEntity == null)
			return;

		if (m_activeEntity != null && m_moveAxis.magnitude != 0) {
			Vector3 camPos = Camera.main.transform.position;
			camPos.Scale(m_moveAxis);

			Vector3 newPos = m_activeEntity.transform.position;
			if (m_moveAxis.x == 1)
				newPos.x = camPos.x + m_offset.x;
			if (m_moveAxis.y == 1)
				newPos.y = camPos.y + m_offset.y;
			if (m_moveAxis.z == 1)
				newPos.z = camPos.z + m_offset.z;

			m_activeEntity.transform.position = newPos;
		}
	}

	void createEntityClassAndInstanceAtMousePos()
	{
		EntityClass entityClass = new EntityClass();
		VoxelObjectRoot root = new VoxelObjectRoot();
		root.voxelObjects.Add(new VoxelObject(0, 4));
		entityClass.setVoxelObjectRoot(root);

		Vector3 worldPos;
		GameObject gameObject;
		if (!getRayWorldPos(out worldPos, out gameObject))
			return;

		EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, worldPos);
		Root.instance.notificationManager.notifyEntityInstanceDescriptionAdded(desc);
	}

	void CreateNewEntityTool()
	{
		Vector3 worldPos;
		GameObject gameObject;
		if (!getRayWorldPos(out worldPos, out gameObject))
			return;

		EntityClass entityClass = Root.instance.player.entityClassInUse;
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
		} else if (Input.GetKey(KeyCode.LeftApple) || Input.GetKey(KeyCode.LeftControl)) {
			EntityInstance entityInstance = gameObject.GetComponent<EntityInstance>();
			if (entityInstance) {
				Root.instance.notificationManager.notifyEntityInstanceDescriptionRemoved(entityInstance.entityInstanceDescription);
			}
		} else {
			EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, worldPos);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionAdded(desc);
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
