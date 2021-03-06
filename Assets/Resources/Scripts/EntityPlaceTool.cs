﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityPlaceTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	Vector3 m_prevPlayerPos;
	float m_prevPlayerXRotation;
	Quaternion m_prevPlayerRotation;

	public void OnEnable()
	{
		resetToolState();
		Root.instance.player.setWalkSpeed(1);
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
		if (Input.GetMouseButtonDown(0))
			Root.instance.player.unselectAllEntityInstances();

		updateMove();

		if (Root.instance.entityToolManager.playerIdle())
			Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
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

		// Calculate how much the head has tilted left/right
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		//playerPosDelta.y = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x);
		float yRotDelta = Mathf.DeltaAngle(playerRotation.eulerAngles.y, m_prevPlayerRotation.eulerAngles.y);
		m_prevPlayerRotation = playerRotation;

		playerPosDelta.Scale(new Vector3(1, 0.1f, 1));
		yRotDelta *= 4;

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.instance.transform.Rotate(0, yRotDelta, 0, Space.Self);
			desc.instance.transform.position += playerPosDelta;
			desc.worldPos = desc.instance.transform.position;
			//desc.rotation = desc.instance.transform.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc, EntityInstanceDescription.DirtyFlags.Transform);
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