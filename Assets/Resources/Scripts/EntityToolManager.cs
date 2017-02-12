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
	public GameObject paintToolGo;

	public float offsetZ = 5f;
	public float offsetY = -2f;
	public float rotation = 0f;

	[HideInInspector] public EntitySelectionTool selectionTool;
	[HideInInspector] public EntityCreateTool createTool;
	[HideInInspector] public EntityMoveTool moveTool;
	[HideInInspector] public EntityRotateTool rotateTool;
	[HideInInspector] public EntityPlaceTool placeTool;
	[HideInInspector] public EntityPainterTool painterTool;

	List<GameObject> m_toolBar;
	int m_toolBarIndex = 0;

	GameObject m_buttonUnderPointer;
	int m_buttonUnderPointerFrameTime;
	PointerEventData m_ped = new PointerEventData(null);

	Vector3 m_lastHeadPos;
	Vector3 m_lastHeadDirection;
	Quaternion m_prevPlayerRotation;

	float m_idleTime;
	Quaternion m_alignmentRotation;
	Vector3 m_alignmentPosition;
	bool m_idleTimerRunning;

	void Awake()
	{
		// Only show entity menus when there is a entity selection
		GetComponent<Canvas>().enabled = false;

		selectionTool = selectionToolGo.GetComponent<EntitySelectionTool>();
		createTool = moveToolGo.GetComponent<EntityCreateTool>();
		moveTool = moveToolGo.GetComponent<EntityMoveTool>();
		rotateTool = rotateToolGo.GetComponent<EntityRotateTool>();
		placeTool = rotateToolGo.GetComponent<EntityPlaceTool>();
		painterTool = rotateToolGo.GetComponent<EntityPainterTool>();

		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	void OnDisable()
	{
		// When we disable the fps tools menu, clear the selection so that we
		// don't jump directly into a tool upon return
		Root.instance.player.unselectAllEntityInstances();
	}

	void Start()
	{
		m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);

		initToolBar();
		deactivateAllTools();
		setTool(0);
	}

	void Update()
	{
		if (Input.anyKey)
			setToolOnKeyPress();
	}

	void initToolBar()
	{
		m_toolBar = new List<GameObject>();
		m_toolBar.Add(createToolGo);
		m_toolBar.Add(moveToolGo);
		m_toolBar.Add(rotateToolGo);
		m_toolBar.Add(paintToolGo);
	}

	public void deactivateAllTools()
	{
		foreach (GameObject go in m_toolBar)
			go.SetActive(false);

		resetToolHelpers();
	}

	void setToolOnKeyPress()
	{
		if (Input.GetKeyDown(KeyCode.E))
			setTool(m_toolBarIndex < m_toolBar.Count - 1 ? m_toolBarIndex + 1 : 0);
		else if (Input.GetKeyDown(KeyCode.Q))
			setTool(m_toolBarIndex > 0 ? m_toolBarIndex - 1 : m_toolBar.Count - 1);
	}

	public void setTool(int index)
	{
		m_toolBar[m_toolBarIndex].SetActive(false);
		m_toolBarIndex = index;
		resetToolHelpers();
		m_toolBar[m_toolBarIndex].SetActive(true);
		print("Tool: " + m_toolBar[m_toolBarIndex]);
	}

	public void setTool(GameObject toolGo)
	{
		setTool(m_toolBar.FindIndex(t => t == toolGo));
	}

	public void repositionMenuAccordingToSelection(List<EntityInstanceDescription> selection)
	{
		GetComponent<Canvas>().enabled = selection.Count != 0;	
		if (selection.Count != 0 && selectionTool.lastHit.normal.magnitude != 0) {
			//transform.SetParent(selection[0].instance.transform);
			transform.position = selectionTool.lastHit.point;
			transform.rotation = Quaternion.LookRotation(selectionTool.lastHit.normal * -1, transform.parent.up);
		}
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		resetToolHelpers();
		repositionMenuAccordingToSelection(newSelection);
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

	public bool playerIdle(float timeout = 0.2f)
	{
		Quaternion rotation = Root.instance.playerHeadGO.transform.rotation;
		Vector3 position = Root.instance.playerGO.transform.position;

		bool rotationChanged = !rotation.Equals(m_alignmentRotation);
		bool positionChanged = !position.Equals(m_alignmentPosition);

		m_alignmentRotation = rotation;
		m_alignmentPosition = position;

		if (positionChanged || rotationChanged) {
			m_idleTimerRunning = true;
			m_idleTime = Time.unscaledTime;
		} else if (m_idleTimerRunning && Time.unscaledTime - m_idleTime > timeout) {
			m_idleTimerRunning = false;
			return true;
		}
		return false;
	}

	public void resetToolHelpers()
	{
		Transform headTransform = Root.instance.playerHeadGO.transform;
		m_lastHeadPos = headTransform.position;
		m_lastHeadDirection = headTransform.forward;
		m_prevPlayerRotation = headTransform.rotation;
	}

	public Vector2 getPlayerHeadMovement()
	{
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		float yMovement = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x) * 0.05f;
		m_prevPlayerRotation = playerRotation;

		return new Vector2(0, yMovement);
	}

	public Vector2 getPlayerMovement()
	{
		Transform headTransform = Root.instance.playerHeadGO.transform;
		Vector3 headPos = headTransform.position;
		Vector3 headDir = headTransform.forward;

		Vector3 normalizedHeadPos = headPos - m_lastHeadPos;
		Vector3 ortogonalHeadDir = Vector3.Cross(m_lastHeadDirection, Vector3.up);

		float zMovement = Vector3.Dot(normalizedHeadPos, m_lastHeadDirection);
		float xMovement = Vector3.Dot(normalizedHeadPos, ortogonalHeadDir);

		m_lastHeadPos = headPos;
		m_lastHeadDirection = headDir;

		return new Vector2(xMovement, zMovement);
	}
}

