using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityToolManager : MonoBehaviour, IEntityInstanceSelectionListener
{
	public GameObject selectionToolGo;
	public GameObject moveToolGo;
	public GameObject createToolGo;

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
	}

	public void onSelectionChanged()
	{
		List<EntityInstanceDescription> selectedInstances = Root.instance.player.selectedEntityInstances;
		if (selectedInstances.Count != 0) {
			//transform.SetParent(selectedInstances[0].instance.transform);
			//transform.SetParent(Root.instance.playerGO.transform);
			//transform.localPosition = new Vector3(0, 0, 5);
			Vector3 pos = Root.instance.playerGO.transform.position
				+ (Root.instance.playerGO.transform.forward * 5)
				+ (Root.instance.playerGO.transform.up * -2);
			transform.position = pos;
			transform.rotation = Quaternion.Euler(60, 0, 0);
			transform.LookAt(2 * transform.position - Root.instance.playerGO.transform.position);

			Root.instance.player.currentTool.SetActive(true);
		} else {
			transform.SetParent(null);
			Root.instance.player.currentTool.SetActive(false);
		}
	}
}

