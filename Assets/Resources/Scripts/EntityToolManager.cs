using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityToolManager : MonoBehaviour
{
	public GameObject selectionToolGo;
	public GameObject moveToolGo;

	[HideInInspector]
	public EntitySelectionTool selectionTool;
	[HideInInspector]
	public EntityMoveTool moveTool;

	void Awake()
	{
		selectionTool = selectionToolGo.GetComponent<EntitySelectionTool>();
		moveTool = moveToolGo.GetComponent<EntityMoveTool>();
	}
}

