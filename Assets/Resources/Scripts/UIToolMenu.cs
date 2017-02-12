using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIToolMenu : MonoBehaviour 
{
	public void onAddBlankSheetButtonClicked()
	{
		Root.instance.player.entityClassInUse = Root.instance.entityClassManager.getEntity(0);
		Root.instance.uiManager.setMenuVisible(false);
	}
}
