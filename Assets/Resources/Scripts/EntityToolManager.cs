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

	public Vector3 getPushDirection()
	{
		// Return the object-algined direction the first selected object is being pushed by the user
		Transform mainTransform = Root.instance.player.selectedEntityInstances[0].instance.transform;
		Vector3 direction = Vector3.zero;
		float dist = Mathf.Infinity;
		selectNearest(ref direction, ref dist, mainTransform.forward);
		selectNearest(ref direction, ref dist, mainTransform.right);
		selectNearest(ref direction, ref dist, mainTransform.up);
		selectNearest(ref direction, ref dist, mainTransform.forward * -1);
		selectNearest(ref direction, ref dist, mainTransform.right * -1);
		selectNearest(ref direction, ref dist, mainTransform.up * -1);
		direction.y = 0;
		direction.Normalize();
		return direction;
	}

	void selectNearest(ref Vector3 current, ref float currentDist, Vector3 other)
	{
		Transform playerTransform = Root.instance.playerGO.transform;
		float dist = Vector3.Distance(other, playerTransform.forward);
		if (dist < currentDist) {
			current = other;
			currentDist = dist;
		}
	}
}

