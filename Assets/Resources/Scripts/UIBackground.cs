using UnityEngine;
using System.Collections;

public class UIBackground : MonoBehaviour {

	public void onBackButtonClicked()
	{
		Root.instance.uiManager.pop(false);
	}

}
