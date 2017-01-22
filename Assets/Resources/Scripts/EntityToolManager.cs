using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityToolManager : MonoBehaviour, IEntityInstanceSelectionListener
{
	public GameObject selectionToolGo;
	public GameObject createToolGo;
	public GameObject moveToolGo;
	public GameObject rotateToolGo;
	public GameObject placeToolGo;

	public float offsetZ = 5f;
	public float offsetY = -2f;
	public float rotation = 0f;

	[HideInInspector] public EntitySelectionTool selectionTool;
	[HideInInspector] public EntityCreateTool createTool;
	[HideInInspector] public EntityMoveTool moveTool;
	[HideInInspector] public EntityRotateTool rotateTool;
	[HideInInspector] public EntityPlaceTool placeTool;

	GameObject m_buttonUnderPointer;
	int m_buttonUnderPointerFrameTime;
	PointerEventData m_ped = new PointerEventData(null);

	void Awake()
	{
		// Only show entity menus when there is a entity selection
		GetComponent<Canvas>().enabled = false;

		selectionTool = selectionToolGo.GetComponent<EntitySelectionTool>();
		createTool = moveToolGo.GetComponent<EntityCreateTool>();
		moveTool = moveToolGo.GetComponent<EntityMoveTool>();
		rotateTool = rotateToolGo.GetComponent<EntityRotateTool>();
		placeTool = rotateToolGo.GetComponent<EntityPlaceTool>();

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void Start()
	{
		m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);

		deactivateAllTools();
		activateTool(moveToolGo, false, true);
		activateTool(createToolGo, true, false);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q))
			activateTool(createToolGo, true, false);
		else if (Input.GetKeyDown(KeyCode.E))
			activateTool(selectionToolGo, true, false);
		else if (Input.GetKeyDown(KeyCode.R))
			activateTool(rotateToolGo, false, true);
		else if (Input.GetKeyDown(KeyCode.M))
			activateTool(moveToolGo, false, true);
		else if (Input.GetKeyDown(KeyCode.P))
			activateTool(placeToolGo, false, true);
	}

	public void deactivateAllTools()
	{
		selectionToolGo.SetActive(false);
		createToolGo.SetActive(false);
		moveToolGo.SetActive(false);
		rotateToolGo.SetActive(false);
		placeToolGo.SetActive(false);
	}

	public void activateTool(GameObject tool, bool setAsMainTool, bool setAsSubTool)
	{
		deactivateAllTools();
		tool.SetActive(true);	

		if (setAsMainTool)
			Root.instance.player.mainTool = tool;
		if (setAsSubTool)
			Root.instance.player.subTool = tool;
	}

	public void activateMainTool()
	{
		activateTool(Root.instance.player.mainTool, false, false);
	}

	public void activateSubTool()
	{
		activateTool(Root.instance.player.subTool, false, false);
	}

	public void repositionMenuAccordingToSelection(List<EntityInstanceDescription> selection)
	{
		GetComponent<Canvas>().enabled = selection.Count != 0;	
		if (selection.Count != 0) {
			transform.SetParent(selection[0].instance.transform);
			transform.position = selectionTool.lastHit.point;
			transform.rotation = Quaternion.LookRotation(selectionTool.lastHit.normal * -1, transform.parent.up);
		}
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		repositionMenuAccordingToSelection(newSelection);

		if (newSelection.Count > 0 && Root.instance.player.mainTool.activeSelf)
			activateSubTool();
		else if (newSelection.Count == 0 && Root.instance.player.subTool.activeSelf)
			activateMainTool();
	}

	public GameObject getButtonUnderPointer()
	{
		// Cache raycast result per frame
		if (Time.frameCount == m_buttonUnderPointerFrameTime)
			return m_buttonUnderPointer;

		m_buttonUnderPointer = null;
		m_buttonUnderPointerFrameTime = Time.frameCount;

		List<RaycastResult> results = new List<RaycastResult>();
		GraphicRaycaster gr = GetComponentInParent<GraphicRaycaster>();
		gr.Raycast(m_ped, results);

		foreach (RaycastResult r in results) {
			if (r.gameObject.GetComponent<Button>()) {
				m_buttonUnderPointer = r.gameObject;
				return m_buttonUnderPointer;
			}
		}

		return null;
	}
}

