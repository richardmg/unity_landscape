using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityMoveTool : MonoBehaviour
{
	Vector3 m_dragDistance;
	float dragScale = 0.1f;

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

	public void onMoveLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveLeftOrRight(1);
	}

	public void onMoveRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveLeftOrRight(-1);
	}

	public void onMoveUpButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveUpOrDown(1);
	}

	public void onMoveDownButtonClicked(BaseEventData bed)
	{
		moveUpOrDown(-1);
	}

	public void onMoveInButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOrOut(-1);
	}

	public void onMoveOutButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOrOut(1);
	}

	/***************** DRAG *******************/

	public void onHorizontalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		moveLeftOrRight(-distance * dragScale);
	}

	public void onUpDownDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		moveUpOrDown(distance * dragScale);
	}

	public void onInOutDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		moveInOrOut(-distance * dragScale);
	}

	/***************** MOVE *******************/

	void moveLeftOrRight(float distance)
	{
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			moveX(-distance);
		else if (rotationY == 90)
			moveZ(distance);
		else if (rotationY == 180)
			moveX(distance);
		else if (rotationY == 270)
			moveZ(-distance);
	}

	void moveInOrOut(float distance)
	{
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			moveZ(-distance);
		else if (rotationY == 90)
			moveX(-distance);
		else if (rotationY == 180)
			moveZ(distance);
		else if (rotationY == 270)
			moveX(distance);
	}

	void moveUpOrDown(float distance)
	{
		moveY(distance);
	}

	void moveX(float distance)
	{
		m_dragDistance.x += Root.instance.entityBaseScale.x * distance;
		float alignedDistance = m_dragDistance.x;
		Root.instance.alignToVoxel(ref alignedDistance);
		m_dragDistance.x -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.right * alignedDistance;
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveY(float distance)
	{
		m_dragDistance.y += Root.instance.entityBaseScale.y * distance;
		float alignedDistance = m_dragDistance.y;
		Root.instance.alignToVoxel(ref alignedDistance);
		m_dragDistance.y -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.up * alignedDistance;
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveZ(float distance)
	{
		m_dragDistance.z += Root.instance.entityBaseScale.z * distance;
		float alignedDistance = m_dragDistance.z;
		Root.instance.alignToVoxel(ref alignedDistance);
		m_dragDistance.z -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.forward * alignedDistance;
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}
}
