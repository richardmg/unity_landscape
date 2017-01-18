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
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			rotateZ(-distance);
		else if (rotationY == 90)
			rotateX(-distance);
		else if (rotationY == 180)
			rotateZ(distance);
		else if (rotationY == 270)
			rotateX(distance);
	}

	void rotateInOrOut(float distance)
	{
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			rotateX(-distance);
		else if (rotationY == 90)
			rotateZ(distance);
		else if (rotationY == 180)
			rotateX(distance);
		else if (rotationY == 270)
			rotateZ(-distance);
	}

	void rotateZenitLeftOrRight(float distance)
	{
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
	}
}
