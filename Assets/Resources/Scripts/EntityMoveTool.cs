using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	Vector3 m_prevPlayerPos;
	float m_prevPlayerXRotation;
	Quaternion m_prevPlayerRotation;
	float m_idleTime;

	Quaternion m_alignmentRotation;
	Vector3 m_alignmentPosition;
	bool m_alignmentNeeded;

	public void OnEnable()
	{
		m_alignmentNeeded = false;
		Root.instance.player.setWalkSpeed(1);
		resetToolState();
		onSelectionChanged(Root.instance.player.selectedEntityInstances, Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.addEntitySelectionListener(this);
	}

	public void OnDisable()
	{
		Root.instance.player.setDefaultWalkSpeed();
		Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.removeEntitySelectionListener(this);
	}

	void Update()
	{
		updateMove();
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
		playerPosDelta.y = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x);
		m_prevPlayerRotation = playerRotation;

		playerPosDelta.Scale(new Vector3(1, 0.1f, 1));

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

}