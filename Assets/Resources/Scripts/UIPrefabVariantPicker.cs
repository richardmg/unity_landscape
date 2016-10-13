using UnityEngine;
using System.Collections;

public class UIEntityClassPicker : MonoBehaviour {

	public void onNewTreeButtonClicked()
	{
		Root.instance.uiManager.push(Root.instance.uiManager.paintEditorGO, (bool accepted) => {});
	}
}
