using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityToolManager : MonoBehaviour, IEntityInstanceSelectionListener
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

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void Start()
	{
		// Hide all tools not in use
		moveToolGo.SetActive(false);
	}

	public void onSelectionChanged()
	{
		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		if (selectedInstances.Count != 0) {
			transform.SetParent(selectedInstances[0].instance.transform);
			transform.localPosition = Vector3.zero;
			gameObject.SetActive(true);
			Root.instance.player.currentTool.SetActive(true);
		} else {
			transform.SetParent(null);
			gameObject.SetActive(false);
			Root.instance.player.currentTool.SetActive(false);
		}
	}
}

