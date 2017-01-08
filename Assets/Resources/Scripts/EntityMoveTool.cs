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
		EntityInstanceDescription desc = Root.instance.player.selectedEntityInstances[0];
		desc.worldPos.x -= Root.instance.entityBaseScale.x;
		Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
	}

	public void onRightButtonClicked()
	{
		EntityInstanceDescription desc = Root.instance.player.selectedEntityInstances[0];
		desc.worldPos.x += Root.instance.entityBaseScale.x;
		Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
	}

	public void onForwardButtonClicked()
	{
		EntityInstanceDescription desc = Root.instance.player.selectedEntityInstances[0];
		desc.worldPos.z -= Root.instance.entityBaseScale.z;
		Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
	}

	public void onBackwardButtonClicked()
	{
		EntityInstanceDescription desc = Root.instance.player.selectedEntityInstances[0];
		desc.worldPos.z += Root.instance.entityBaseScale.z;
		Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
	}
}
