﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
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
		Root.instance.entityToolManager.updateAlignment();
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