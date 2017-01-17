using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityMoveTool : MonoBehaviour
{
	Vector3 m_dragDistance;
	float dragScale = 0.1f;
	bool flipped = false;
	bool backSide = false;

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

		Vector3 menuDirection = Root.instance.entityToolManagerGO.transform.forward;
		print(menuDirection.x);
		int x = Mathf.RoundToInt(menuDirection.x);
		int y = Mathf.RoundToInt(menuDirection.y);
		int z = Mathf.RoundToInt(menuDirection.z);

		if (z == 1)
			moveX(-1);
		else if (z == -1)
			moveX(1);
		else if (x == 1)
			moveZ(1);
		else if (x == -1)
			moveZ(-1);
	}

	public void onMoveRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		if (flipped)
			moveZ(1);
		else
			moveX(1);
	}

	public void onMoveUpButtonClicked(BaseEventData bed)
	{
	}

	public void onMoveDownButtonClicked(BaseEventData bed)
	{
	}

	public void onMoveInButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		if (flipped)
			moveX(1);
		else
			moveZ(1);
	}

	public void onMoveOutButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		if (flipped)
			moveX(-1);
		else
			moveZ(-1);
	}

	/***************** DRAG *******************/

	public void OnHorizontalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		distance *= dragScale;

		if (pointerData.clickCount == -1)
			backSide = playerIsOnBackside();
		if (backSide)
			distance *= -1;

		if (flipped)
			moveZ(distance);
		else
			moveX(distance);
	}

	public void OnVerticalDrag(BaseEventData bed)
	{
	}

	public void OnInOutDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		distance *= dragScale;

		if (pointerData.clickCount == -1)
			backSide = playerIsOnBackside();
		if (backSide)
			distance *= -1;

		if (flipped)
			moveX(distance);
		else
			moveZ(distance);
	}

	/***************** MOVE *******************/

	void moveX(float distance)
	{
		m_dragDistance.x += Root.instance.entityBaseScale.x * distance;
		float alignedDistance = m_dragDistance.x;
		Root.instance.alignToVoxel(ref alignedDistance);
		m_dragDistance.x -= alignedDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.x += alignedDistance;
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
			desc.worldPos.y += alignedDistance;
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
			desc.worldPos.z += alignedDistance;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	/***************** ROTATE *******************/

	public void onRotateLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
	}

	public void onRotateRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
	}

	public void onRotateUpButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.rotation = Quaternion.Euler(22.5f, 0, 0) * desc.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onRotateDownButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.rotation = Quaternion.Euler(-22.5f, 0, 0) * desc.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public bool playerIsOnBackside()
	{
		// Todo: calculate onMousePress, and not for each dragEvent (since this
		// will be confusing when target passes player
		return flipped ?
			Root.instance.player.transform.position.x > transform.position.x :
			Root.instance.player.transform.position.z > transform.position.z;
	}
}
