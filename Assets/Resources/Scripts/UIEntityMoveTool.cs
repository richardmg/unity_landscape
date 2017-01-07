using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntityMoveTool : MonoBehaviour {

	public void onLeftButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.gameObjectInUse;
		Vector3 pos = selectedGo.transform.position;
		pos.x -= Root.instance.entityBaseScale.x;
		selectedGo.transform.position = pos;
	}

	public void onRightButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.gameObjectInUse;
		Vector3 pos = selectedGo.transform.position;
		pos.x += Root.instance.entityBaseScale.x;
		selectedGo.transform.position = pos;
	}

	public void onForwardButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.gameObjectInUse;
		Vector3 pos = selectedGo.transform.position;
		pos.z += Root.instance.entityBaseScale.z;
		selectedGo.transform.position = pos;
	}

	public void onBackwardButtonClicked()
	{
		GameObject selectedGo = Root.instance.player.gameObjectInUse;
		Vector3 pos = selectedGo.transform.position;
		pos.z -= Root.instance.entityBaseScale.z;
		selectedGo.transform.position = pos;
	}

	public void onDoneButtonClicked()
	{
		Root.instance.player.gameObjectInUse = null;
		GameObject ui = Root.instance.entityUiGO;
		ui.transform.SetParent(null);
		ui.SetActive(false);
	}
}
