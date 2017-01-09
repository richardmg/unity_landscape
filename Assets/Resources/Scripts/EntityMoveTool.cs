using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityMoveTool : MonoBehaviour
{

	Vector3 m_targetPos;

	public void OnEnable()
	{
		if (Root.instance.player.selectedEntityInstances.Count > 0)
			m_targetPos = Root.instance.player.selectedEntityInstances[0].worldPos;
	}

	public void onDoneButtonClicked()
	{
		// todo: Move to selection tool?
		Root.instance.player.unselectEntityInstance(null);
	}

	public void onMoveLeftButtonClicked()
	{
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
		Vector3 relativePos = m_targetPos - transform.position;

		bool playerInFront = relativePos.z > 0;
		bool playerOnRight = relativePos.x > 0;
		bool moreFrontThanSide = Mathf.Abs(relativePos.z) > Mathf.Abs(relativePos.x);

		if (moreFrontThanSide)
			moveAlongX(playerInFront ? -leftButton : leftButton);
		else
			moveAlongZ(playerOnRight ? leftButton : -leftButton);
	}

	void moveAlongX(int direction)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.x += Root.instance.entityBaseScale.x * direction;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveAlongZ(int direction)
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.z += Root.instance.entityBaseScale.x * direction;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}
}
