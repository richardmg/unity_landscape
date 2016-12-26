using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBackground : MonoBehaviour {

	public void onOkButtonClicked()
	{
		Root.instance.uiManager.pop(true);
	}

	public void onCancelButtonClicked()
	{
		Root.instance.uiManager.pop(false);
	}

}
