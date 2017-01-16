using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityToolManager : MonoBehaviour, IEntityInstanceSelectionListener
{
	public GameObject selectionToolGo;
	public GameObject moveToolGo;
	public GameObject createToolGo;

	public float offsetZ = 5f;
	public float offsetY = -2f;
	public float rotation = 0f;

	[HideInInspector]
	public EntitySelectionTool selectionTool;
	[HideInInspector]
	public EntityMoveTool moveTool;
	[HideInInspector]
	public EntityCreateTool createTool;

	void Awake()
	{
		// Only show entity menus when there is a entity selection
		GetComponent<Canvas>().enabled = false;

		selectionTool = selectionToolGo.GetComponent<EntitySelectionTool>();
		moveTool = moveToolGo.GetComponent<EntityMoveTool>();
		createTool = moveToolGo.GetComponent<EntityCreateTool>();

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void Start()
	{
		deactivateAllTools();

		// Selection tool is handled on the side, and is always active
		selectionToolGo.SetActive(true);
		// Start with create tool active
		activateTool(createToolGo);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
			activateTool(createToolGo);
		else if (Input.GetKeyDown(KeyCode.Alpha2))
			activateTool(moveToolGo);

		moveMenuToSelection();
	}

	public void deactivateAllTools()
	{
		createToolGo.SetActive(false);
		moveToolGo.SetActive(false);
	}

	public void activateTool(GameObject tool)
	{
		Root.instance.player.currentTool = tool;
		deactivateAllTools();
		tool.SetActive(true);	
		print("Activated " + tool.name);
	}

	public void onSelectionChanged()
	{
		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		GetComponent<Canvas>().enabled = selectedInstances.Count != 0;	
		if (selectedInstances.Count != 0)
			moveMenuToSelection();
	}

	void moveMenuToSelection()
	{
		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		if (selectedInstances.Count != 0)
			transform.position = selectedInstances[0].instance.transform.position;
	}
}

