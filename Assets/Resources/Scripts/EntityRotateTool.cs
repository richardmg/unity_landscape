using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityRotateTool : MonoBehaviour
{
	Vector3 m_dragDistance;
	float dragScale = 0.1f;
	float angleStep = 10f;

	public void OnEnable()
	{
		m_dragDistance = Vector3.zero;
	}

	void Update()
	{
		if (Root.instance.entityToolManager.getButtonUnderPointer() == null && Input.GetMouseButtonDown(0))
			Root.instance.entityToolManager.selectionTool.updateSelection();
	}

	/***************** CLICK *******************/

	public void onRotateLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateLeftOrRight(-1);
	}

	public void onRotateRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateLeftOrRight(1);
	}

	public void onRotateZenitLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateZenitLeftOrRight(-1);
	}

	public void onRotateZenitRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateZenitLeftOrRight(1);
	}

	public void onRotateInButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateInOrOut(-1);
	}

	public void onRotateOutButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		rotateInOrOut(1);
	}

	/***************** DRAG *******************/

	public void onHorizontalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		rotateLeftOrRight(distance * dragScale);
	}

	public void onZenitDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		rotateZenitLeftOrRight(distance * dragScale);
	}

	public void onInOutDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		rotateInOrOut(-distance * dragScale);
	}

	/***************** MOVE *******************/

	void fillWithMenuDirection(out int x, out int y, out int z)
	{
		Vector3 menuDirection = Root.instance.entityToolManagerGO.transform.forward;
		x = Mathf.RoundToInt(menuDirection.x);
		y = Mathf.RoundToInt(menuDirection.y);
		z = Mathf.RoundToInt(menuDirection.z);
	}

	void rotateLeftOrRight(float distance)
	{
		int x, y, z;
		fillWithMenuDirection(out x, out y, out z);

		if (z == 1)
			rotateZ(-distance);
		else if (z == -1)
			rotateZ(distance);
		else if (x == 1)
			rotateX(-distance);
		else if (x == -1)
			rotateX(distance);
		else if (y == 1)
			rotateZ(distance);
		else
			rotateZ(-distance);
	}

	void rotateInOrOut(float distance)
	{
		int x, y, z;
		fillWithMenuDirection(out x, out y, out z);

		if (z == 1)
			rotateX(-distance);
		else if (z == -1)
			rotateX(distance);
		else if (x == 1)
			rotateZ(-distance);
		else if (x == -1)
			rotateZ(distance);
		else if (y == 1)
			rotateX(distance);
		else
			rotateX(-distance);
	}

	void rotateZenitLeftOrRight(float distance)
	{
		int x, y, z;
		fillWithMenuDirection(out x, out y, out z);

		if (z == 1)
			rotateY(-distance);
		else if (z == -1)
			rotateY(-distance);
		else if (x == 1)
			rotateY(-distance);
		else if (x == -1)
			rotateY(-distance);
		else if (y == 1)
			rotateY(-distance);
		else
			rotateY(-distance);
	}

	void rotateX(float distance)
	{
		m_dragDistance.x += angleStep * distance;
		float alignedDistance = m_dragDistance.x;
		alignedDistance = Mathf.Round(alignedDistance / angleStep) * angleStep;
		m_dragDistance.x -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.Rotate(alignedDistance, 0, 0, Space.Self);
			desc.rotation = desc.instance.transform.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}

		syncMenuPosition(new Vector3(alignedDistance, 0, 0));
	}

	void rotateY(float distance)
	{
		m_dragDistance.y += angleStep * distance;
		float alignedDistance = m_dragDistance.y;
		alignedDistance = Mathf.Round(alignedDistance / angleStep) * angleStep;
		m_dragDistance.y -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.Rotate(0, alignedDistance, 0, Space.Self);
			desc.rotation = desc.instance.transform.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}

		syncMenuPosition(new Vector3(0, alignedDistance, 0));
	}

	void rotateZ(float distance)
	{
		m_dragDistance.z += angleStep * distance;
		float alignedDistance = m_dragDistance.z;
		alignedDistance = Mathf.Round(alignedDistance / angleStep) * angleStep;
		m_dragDistance.z -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.Rotate(0, 0, alignedDistance, Space.Self);
			desc.rotation = desc.instance.transform.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}

		syncMenuPosition(new Vector3(0, 0, alignedDistance));
	}

	void syncMenuPosition(Vector3 delta)
	{
//		Vector3 pos = Root.instance.entityToolManagerGO.transform.position;
//		Root.instance.entityToolManagerGO.transform.position = pos + delta;
	}
}
