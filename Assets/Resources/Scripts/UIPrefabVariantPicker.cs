using UnityEngine;
using System.Collections;

public class UIPrefabVariantPicker : MonoBehaviour {

	public void newVariantButtonClicked()
	{
		Root.instance.uiManager.push(Root.instance.uiManager.paintEditorGO, (bool accepted) => {});
	}

	public void newTreeButtonClicked()
	{
		// Redirect button click here
		// Create PrefabVariant, make it current
		// Open paint editor
		Root.instance.uiManager.push(Root.instance.uiManager.paintEditorGO, (bool accepted) => {});
	}
}
