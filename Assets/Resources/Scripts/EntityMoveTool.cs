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
		GameObject selectedGo = Root.instance.player.selectedEntityInstances[0].gameObject;
		Vector3 pos = selectedGo.transform.position;
		pos.x -= Root.instance.entityBaseScale.x;
		selectedGo.transform.position = pos;
	}

	public void onRightButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.selectedEntityInstances[0].gameObject;
		Vector3 pos = selectedGo.transform.position;
		pos.x += Root.instance.entityBaseScale.x;
		selectedGo.transform.position = pos;
	}

	public void onForwardButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.selectedEntityInstances[0].gameObject;
		Vector3 pos = selectedGo.transform.position;
		pos.z += Root.instance.entityBaseScale.z;
		selectedGo.transform.position = pos;
	}

	public void onBackwardButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.selectedEntityInstances[0].gameObject;
		Vector3 pos = selectedGo.transform.position;
		pos.z -= Root.instance.entityBaseScale.z;
		selectedGo.transform.position = pos;
	}
}
