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

	public void onDoneButtonClicked()
	{
		// todo: Move to selection tool?
		Root.instance.player.unselectEntityInstance(null);
	}

	public void onMoveLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging) {
			print("dragging");
			return;
		}
		handleLeftOrRightButtonClicked(1);
	}

	public void onMoveRightButtonClicked()
	{
		handleLeftOrRightButtonClicked(-1);
	}

	public void onMoveUpButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.y += Root.instance.entityBaseScale.y;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onMoveDownButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.y -= Root.instance.entityBaseScale.y;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onRotateLeftButtonClicked()
	{
	}

	public void onRotateRightButtonClicked()
	{
	}

	public void onRotateUpButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.rotation = Quaternion.Euler(22.5f, 0, 0) * desc.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onRotateDownButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.rotation = Quaternion.Euler(-22.5f, 0, 0) * desc.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void handleLeftOrRightButtonClicked(int leftButton)
	{
		Vector3 relativePos = m_targetStartPos - transform.position;

		bool playerInFront = relativePos.z > 0;
		bool playerOnRight = relativePos.x > 0;
		bool moreFrontThanSide = Mathf.Abs(relativePos.z) > Mathf.Abs(relativePos.x);

		if (moreFrontThanSide)
			moveAlongX(playerInFront ? leftButton : -leftButton);
		else
			moveAlongZ(playerOnRight ? -leftButton : leftButton);
	}

	void moveAlongX(float direction)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			m_targetPos.x += Root.instance.entityBaseScale.x * direction;
			desc.worldPos.x = m_targetPos.x;
			Root.instance.alignToVoxel(ref desc.worldPos.x);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveAlongZ(float direction)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			m_targetPos.z += Root.instance.entityBaseScale.z * direction;
			desc.worldPos.z = m_targetPos.z;
			Root.instance.alignToVoxel(ref desc.worldPos.z);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void OnHorizontalDrag(BaseEventData bed)
	{
		// get distance dragged. if more than one scale length, call onMoveLeftButtonClicked
		PointerEventData pointerData = bed as PointerEventData;
		moveAlongX(pointerData.delta.x * dragScale);
	}

	public void OnVerticalDrag(BaseEventData bed)
	{
		// get distance dragged. if more than one scale length, call onMoveLeftButtonClicked
		PointerEventData pointerData = bed as PointerEventData;
		moveAlongZ(pointerData.delta.y * dragScale);
	}
}
