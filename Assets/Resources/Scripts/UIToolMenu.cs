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
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.createToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onPaintButtonClicked()
	{
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.paintToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onPaint3DButtonClicked()
	{
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.paint3DToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onMoveButtonClicked()
	{
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.moveToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onRotateButtonClicked()
	{
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.rotateToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onDestroyButtonClicked()
	{
		Root.instance.entityToolManager.setTool(Root.instance.entityToolManager.destroyToolGo);
		Root.instance.uiManager.setMenuVisible(false);
	}
}
