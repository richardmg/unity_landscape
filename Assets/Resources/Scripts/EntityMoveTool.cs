using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityMoveTool : MonoBehaviour
{

	Vector3 m_targetStartPos;
	Vector3 m_targetPos;
	float dragScale = 0.1f;

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
		moveHorizontal(1);
	}

	public void onMoveRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveHorizontal(-1);
	}

	public void onMoveUpButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveVertical(1);
	}

	public void onMoveDownButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveVertical(-1);
	}

	public void onMoveInButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOut(1);
	}

	public void onMoveOutButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOut(-1);
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

	/***************** DRAG *******************/

	public void OnHorizontalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		moveX(pointerData.delta.x * dragScale);
	}

	public void OnVerticalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		moveY(pointerData.delta.y * dragScale);
	}

	public void OnInOutDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		moveInOut(pointerData.delta.y * dragScale);
	}

	/***************** MOVE *******************/

	public void moveHorizontal(float distance)
	{
		Vector3 relativePos = m_targetStartPos - transform.position;

		bool playerInFront = relativePos.z > 0;
		bool playerOnRight = relativePos.x > 0;
		bool moreFrontThanSide = Mathf.Abs(relativePos.z) > Mathf.Abs(relativePos.x);

		if (moreFrontThanSide)
			moveX(playerInFront ? distance : -distance);
		else
			moveZ(playerOnRight ? -distance : distance);
	}

	public void moveVertical(float distance)
	{
		moveY(distance);
	}

	public void moveInOut(float distance)
	{
		Vector3 relativePos = m_targetStartPos - transform.position;

		bool playerInFront = relativePos.z > 0;
		bool playerOnRight = relativePos.x > 0;
		bool moreFrontThanSide = Mathf.Abs(relativePos.z) > Mathf.Abs(relativePos.x);

		if (moreFrontThanSide)
			moveZ(playerInFront ? distance : -distance);
		else
			moveX(playerOnRight ? -distance : distance);
	}

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
}
