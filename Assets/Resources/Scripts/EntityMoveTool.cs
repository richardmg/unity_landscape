using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityMoveTool : MonoBehaviour
{
	public void onDoneButtonClicked()
	{
		// todo: Move to selection tool?
		Root.instance.player.unselectEntityInstance(null);
	}

	public void onLeftButtonClicked()
	{
		handleLeftOrRightButtonClicked(1);
	}

	public void onRightButtonClicked()
	{
		handleLeftOrRightButtonClicked(-1);
	}

	public void onUpButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.y += Root.instance.entityBaseScale.y;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onDownButtonClicked()
	{
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.worldPos.y -= Root.instance.entityBaseScale.y;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void handleLeftOrRightButtonClicked(int leftButton)
	{
		EntityInstanceDescription mainDesc = Root.instance.player.selectedEntityInstances[0];
		Vector3 targetPos = mainDesc.worldPos;
		Vector3 playerPos = Root.instance.playerGO.transform.position;
		Vector3 relativePos = targetPos - playerPos;

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
