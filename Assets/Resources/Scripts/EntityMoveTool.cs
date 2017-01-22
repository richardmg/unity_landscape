using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	Vector3 m_dragDistance;
	Vector3 m_prevPlayerPos;
	float m_prevPlayerXRotation;
	Quaternion m_prevPlayerRotation;
	float m_idleTime;

	float dragScale = 0.1f;

	bool inHoriontalDrag = false;
	bool inDrag = false;

	Quaternion m_alignmentRotation;
	Vector3 m_alignmentPosition;
	bool m_alignmentNeeded;

	ToolMode m_currentMode;

	public const ToolMode kMoveObject = 0;
	public const ToolMode kRotateObject = 1;
	public const ToolMode kMovePlayer = 2;

	public void OnEnable()
	{
		m_currentMode = kMoveObject;
		m_dragDistance = Vector3.zero;
		m_alignmentNeeded = false;
		resetToolState();
		onSelectionChanged(Root.instance.player.selectedEntityInstances, Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	public void OnDisable()
	{
		m_currentMode = kMovePlayer;
		Root.instance.player.setDefaultWalkSpeed();
		Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.removeEntitySelectionListener(this);
	}

	void Update()
	{
		// Resolve which objects should be seleced
		if (Root.instance.entityToolManager.getButtonUnderPointer() == null && Input.GetMouseButtonDown(0))
			Root.instance.entityToolManager.selectionTool.updateSelection();

		if (Input.GetKeyDown(KeyCode.E)) {
			m_currentMode = (m_currentMode == kMoveObject) ? kMovePlayer : kMoveObject;
			resetToolState();
		} else if (Input.GetKeyDown(KeyCode.R)) {
			m_currentMode = (m_currentMode == kRotateObject) ? kMovePlayer : kRotateObject;
			resetToolState();
		}

		if (m_currentMode == kMoveObject) {
			Root.instance.player.setWalkSpeed(1);
			updateMove();
		} else if (m_currentMode == kRotateObject) {
			Root.instance.player.setWalkSpeed(1);
			updateRotate();
		} else {
			Root.instance.player.setDefaultWalkSpeed();
		}

		updateAlignment();
	}

	void updateMove()
	{
		// Get the players position, but ignore height
		float startHeight = m_prevPlayerPos.y;
		Vector3 playerPos = Root.instance.playerGO.transform.position;
		playerPos.y = startHeight;

		// Calculate how much the player moved sine last update
		Vector3 playerPosDelta = playerPos - m_prevPlayerPos;
		m_prevPlayerPos = playerPos;

		// Calculate how much the head has tilted up/down
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		float xAngleDelta = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x);
		m_prevPlayerRotation = playerRotation;
		playerPosDelta.y = xAngleDelta * 0.1f;

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += playerPosDelta;
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void updateAlignment()
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
			// Align selected objects
			foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
				Root.instance.alignmentManager.align(desc.instance.transform);
				desc.worldPos = desc.instance.transform.position;
				desc.rotation = desc.instance.transform.rotation;
				Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
			}
			m_alignmentNeeded = false;
		}
	}

	void updateRotate()
	{
	}

	public void resetToolState()
	{
		m_prevPlayerPos = Root.instance.playerGO.transform.position;
		m_prevPlayerRotation = Root.instance.playerHeadGO.transform.rotation;
		m_idleTime = Time.unscaledTime;
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		Root.instance.alignmentManager.align(oldSelection);
	}

	/***************** CLICK *******************/

	public void onMoveLeftButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveLeftOrRight(1);
	}

	public void onMoveRightButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveLeftOrRight(-1);
	}

	public void onMoveUpButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveUpOrDown(1);
	}

	public void onMoveDownButtonClicked(BaseEventData bed)
	{
		moveUpOrDown(-1);
	}

	public void onMoveInButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOrOut(-1);
	}

	public void onMoveOutButtonClicked(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		if (pointerData.dragging)
			return;
		moveInOrOut(1);
	}

	/***************** DRAG *******************/

	void updateDragStatus(PointerEventData pointerData)
	{
		bool pointerMoved = (pointerData.delta.x != 0 || pointerData.delta.y != 0);

		if (inDrag) {
			if (!pointerMoved)
				inDrag = false;
		} else if (pointerMoved) {
			inDrag = true;
			inHoriontalDrag = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y);
		}
	}

	public void onHorizontalDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		updateDragStatus(pointerData);

		if (inDrag) {
			if (inHoriontalDrag)
				moveLeftOrRight(-pointerData.delta.x * dragScale);
			else
				moveInOrOut(-pointerData.delta.y * dragScale);
		}
	}

	public void onUpDownDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		moveUpOrDown(distance * dragScale);
	}

	public void onInOutDrag(BaseEventData bed)
	{
		PointerEventData pointerData = bed as PointerEventData;
		float distance = Mathf.Abs(pointerData.delta.x) > Mathf.Abs(pointerData.delta.y) ? pointerData.delta.x : pointerData.delta.y;
		moveInOrOut(-distance * dragScale);
	}

	/***************** MOVE *******************/

	void moveLeftOrRight(float distance)
	{
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			moveX(-distance);
		else if (rotationY == 90)
			moveZ(distance);
		else if (rotationY == 180)
			moveX(distance);
		else if (rotationY == 270)
			moveZ(-distance);
	}

	void moveInOrOut(float distance)
	{
		Vector3 rotation = Root.instance.entityToolManagerGO.transform.localRotation.eulerAngles;
		int rotationY = Mathf.RoundToInt(rotation.y); 

		if (rotationY == 0)
			moveZ(-distance);
		else if (rotationY == 90)
			moveX(-distance);
		else if (rotationY == 180)
			moveZ(distance);
		else if (rotationY == 270)
			moveX(distance);
	}

	void moveUpOrDown(float distance)
	{
		moveY(distance);
	}

	void moveX(float distance)
	{
		m_dragDistance.x += Root.instance.alignmentManager.voxelSize.x * distance;
		float dragDistance = Root.instance.alignmentManager.align(m_dragDistance.x);
		m_dragDistance.x -= dragDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.right * dragDistance;
			Root.instance.alignmentManager.align(desc.instance.transform);
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveY(float distance)
	{
		m_dragDistance.y += Root.instance.alignmentManager.voxelSize.y * distance;
		float dragDistance = Root.instance.alignmentManager.align(m_dragDistance.y);
		m_dragDistance.y -= dragDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.up * dragDistance;
			Root.instance.alignmentManager.align(desc.instance.transform);
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	void moveZ(float distance)
	{
		m_dragDistance.z += Root.instance.alignmentManager.voxelSize.z * distance;
		float dragDistance = Root.instance.alignmentManager.align(m_dragDistance.z);
		m_dragDistance.z -= dragDistance;

		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.position += desc.instance.transform.forward * dragDistance;
			Root.instance.alignmentManager.align(desc.instance.transform);
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}
}
