using UnityEngine;
using System.Collections;

public class UIPrefabVariantPicker : MonoBehaviour {

	public void onNewTreeButtonClicked()
	{
		Root.instance.uiManager.push(Root.instance.uiManager.paintEditorGO, (bool accepted) => {});
	}
}
