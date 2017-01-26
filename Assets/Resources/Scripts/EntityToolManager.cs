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

	Vector3 m_lastHeadPos;
	Vector3 m_lastHeadDirection;
	Quaternion m_prevPlayerRotation;

	float m_idleTime;
	Quaternion m_alignmentRotation;
	Vector3 m_alignmentPosition;
	bool m_alignmentNeeded;

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
		setSubTool(moveToolGo);
		setMainTool(createToolGo);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.C)) {
			setMainTool(createToolGo);
		} else if (Input.GetKeyDown(KeyCode.R)) {
			setMainTool(selectionToolGo);
			setSubTool(rotateToolGo);
		} else if (Input.GetKeyDown(KeyCode.M)) {
			setMainTool(selectionToolGo);
			setSubTool(moveToolGo);
		} else if (Input.GetKeyDown(KeyCode.P)) {
			setMainTool(selectionToolGo);
			setSubTool(placeToolGo);
		}
	}

	public void deactivateAllTools()
	{
		selectionToolGo.SetActive(false);
		createToolGo.SetActive(false);
		moveToolGo.SetActive(false);
		rotateToolGo.SetActive(false);
		placeToolGo.SetActive(false);

		resetToolHelpers();
	}

	public void setSubTool(GameObject tool)
	{
		Root.instance.player.subTool = tool;
		if (Root.instance.player.selectedEntityInstances.Count > 0) {
			deactivateAllTools();
			tool.SetActive(true);	
		}
	}

	public void setMainTool(GameObject tool)
	{
		Root.instance.player.mainTool = tool;
		if (Root.instance.player.selectedEntityInstances.Count == 0) {
			deactivateAllTools();
			tool.SetActive(true);	
		}
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
		repositionMenuAccordingToSelection(newSelection);
		setSubTool(Root.instance.player.subTool);
		setMainTool(Root.instance.player.mainTool);
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

	public void updateAlignment()
	{
		Quaternion rotation = Root.instance.playerHeadGO.transform.rotation;
		Vector3 position = Root.instance.playerGO.transform.position;

		bool rotationChanged = !rotation.Equals(m_alignmentRotation);
		bool positionChanged = !position.Equals(m_alignmentPosition);

		m_alignmentRotation = rotation;
		m_alignmentPosition = position;

		if (positionChanged || rotationChanged) {
			m_alignmentNeeded = true;
			m_idleTime = Time.unscaledTime;
		} else if (m_alignmentNeeded && Time.unscaledTime - m_idleTime > 0.2f) {
			Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
			m_alignmentNeeded = false;
		}
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

	public Vector3 getPushDirection(Space space)
	{
		Transform pusher = Root.instance.playerGO.transform;
		Transform pushed = Root.instance.player.selectedEntityInstances[0].instance.transform;
		return pusher.getVoxelPushDirection(pushed, space);
	}
}

