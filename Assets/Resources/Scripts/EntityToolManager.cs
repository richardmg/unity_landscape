using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IEntityTool
{
	void setAlternativeMode(bool alternativeMode);
}

public class EntityToolManager : MonoBehaviour, IEntityInstanceSelectionListener
{
	public GameObject selectionToolGo;
	public GameObject createToolGo;
	public GameObject moveToolGo;
	public GameObject rotateToolGo;

	public float offsetZ = 5f;
	public float offsetY = -2f;
	public float rotation = 0f;

	[HideInInspector] public EntitySelectionTool selectionTool;
	[HideInInspector] public EntityCreateTool createTool;
	[HideInInspector] public EntityMoveTool moveTool;
	[HideInInspector] public EntityRotateTool rotateTool;

	GameObject m_switchToolOnUnselect = null;
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

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void Start()
	{
		m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);

		deactivateAllTools();

		// Selection tool is controlled by each
		// tool individually, and is always active
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
		else if (Input.GetKeyDown(KeyCode.Alpha3))
			activateTool(rotateToolGo);
	}

	public void deactivateAllTools()
	{
		createToolGo.SetActive(false);
		moveToolGo.SetActive(false);
		rotateToolGo.SetActive(false);
	}

	public void activateTool(GameObject tool, GameObject switchToolOnUnselect = null)
	{
		m_switchToolOnUnselect = switchToolOnUnselect;	
		Root.instance.player.currentTool = tool;
		deactivateAllTools();
		tool.SetActive(true);	
	}

	public bool activateSwitchTool()
	{
		if (!m_switchToolOnUnselect)
			return false;
		activateTool(m_switchToolOnUnselect);
		return true;
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
		if (newSelection.Count == 0)
			activateSwitchTool();
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

