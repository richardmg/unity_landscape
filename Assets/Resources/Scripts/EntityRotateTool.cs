using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityRotateTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	Vector3 m_prevPlayerPos;
	float m_prevPlayerXRotation;
	Quaternion m_prevPlayerRotation;

	Vector3 m_lastHeadPos;
	Vector3 m_lastHeadDirection;

	public void OnEnable()
	{
		resetToolState();
		Root.instance.player.setWalkSpeed(1);
		onSelectionChanged(Root.instance.player.selectedEntityInstances, Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.addEntitySelectionListener(this);

		m_lastHeadPos = Root.instance.playerHeadGO.transform.position;
		m_lastHeadDirection = Root.instance.playerHeadGO.transform.forward;
	}

	public void OnDisable()
	{
		Root.instance.player.setDefaultWalkSpeed();
		Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.removeEntitySelectionListener(this);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
			Root.instance.player.unselectAllEntityInstances();

		updateRotate();
		Root.instance.entityToolManager.updateAlignment();
	}

	void updateRotate()
	{
		Vector3 headPos = Root.instance.playerHeadGO.transform.position;
		Vector3 headDir = Root.instance.playerHeadGO.transform.forward;

		Vector3 normalizedHeadPos = headPos - m_lastHeadPos;
		Vector3 ortogonalHeadDir = Vector3.Cross(m_lastHeadDirection, Vector3.up);
		float zMovement = Vector3.Dot(normalizedHeadPos, m_lastHeadDirection) * 30;
		float xMovement = Vector3.Dot(normalizedHeadPos, ortogonalHeadDir) * 30;

		m_lastHeadPos = headPos;
		m_lastHeadDirection = headDir;

		// Calculate how much the head has tilted left/right
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		float yMovement = Mathf.DeltaAngle(playerRotation.eulerAngles.y, m_prevPlayerRotation.eulerAngles.y) * 4;
		m_prevPlayerRotation = playerRotation;

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.voxelRotation.x += zMovement;
			desc.voxelRotation.y += xMovement;
			desc.voxelRotation.z += yMovement;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void resetToolState()
	{
		m_prevPlayerPos = Root.instance.playerGO.transform.position;
		m_prevPlayerRotation = Root.instance.playerHeadGO.transform.rotation;
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		Root.instance.alignmentManager.align(oldSelection);
	}

}