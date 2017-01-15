using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityMoveTool : MonoBehaviour
{

	Vector3 m_targetStartPos;
	Vector3 m_targetPos;
	float dragScale = 0.1f;
	bool flipped = false;
	bool backSide = false;

	public void OnEnable()
	{
		if (Root.instance.player.selectedEntityInstances.Count > 0) {
			m_targetStartPos = Root.instance.player.selectedEntityInstances[0].worldPos;
			m_targetPos = m_targetStartPos;
		}
	}

	/***************** CLICK *******************/

	public void onDoneButtonClicked()
	{
		// todo: Move to selection tool?
		Root.instance.player.unselectEntityInstance(null);
	}

	public void onMoveLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		if (flipped)
			moveZ(-1);
		else
			moveX(-1);
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
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			m_targetPos.x += Root.instance.entityBaseScale.x * distance;
			desc.worldPos.x = m_targetPos.x;
			Root.instance.alignToVoxel(ref desc.worldPos.x);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveY(float distance)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			m_targetPos.y += Root.instance.entityBaseScale.y * distance;
			desc.worldPos.y = m_targetPos.y;
			Root.instance.alignToVoxel(ref desc.worldPos.y);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveZ(float distance)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			m_targetPos.z += Root.instance.entityBaseScale.z * distance;
			desc.worldPos.z = m_targetPos.z;
			Root.instance.alignToVoxel(ref desc.worldPos.z);
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
