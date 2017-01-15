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
		selectionTool = selectionToolGo.GetComponent<EntitySelectionTool>();
		moveTool = moveToolGo.GetComponent<EntityMoveTool>();
		createTool = moveToolGo.GetComponent<EntityCreateTool>();

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void Start()
	{
		// Hide all tools not in use
		moveToolGo.SetActive(false);
		selectionToolGo.SetActive(false);
		createToolGo.SetActive(false);
	}

	void Update()
	{
		// Selection and Create tools are exclusive, but one of them is always on
		selectionToolGo.SetActive(Input.GetKey(KeyCode.LeftApple));
		createToolGo.SetActive(!selectionToolGo.activeSelf && !Root.instance.player.currentTool.activeSelf);

		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		if (selectedInstances.Count != 0)
			transform.position = selectedInstances[0].instance.transform.position;
	}

	public void onSelectionChanged()
	{
		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		if (selectedInstances.Count != 0) {
			Root.instance.player.currentTool.SetActive(true);
		} else {
			transform.SetParent(null);
			Root.instance.player.currentTool.SetActive(false);
		}
	}
}

