﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
{
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
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;

		float xMovement, zMovement;
		Root.instance.entityToolManager.getPlayerMovement(out xMovement, out zMovement);

		// Calculate how much the head has tilted up/down
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		float yMovement = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x) * 0.05f;
		m_prevPlayerRotation = playerRotation;

		Vector3 pushDirection = Root.instance.entityToolManager.getPlayerPushDirectionOfFirstSelectedObject();

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			Transform t = desc.instance.transform;
			t.Translate(Vector3.Cross(pushDirection, Vector3.up) * xMovement, Space.World);
			t.Translate(pushDirection * zMovement, Space.World);
			t.Translate(new Vector3(0, yMovement, 0), Space.Self);
			desc.worldPos = t.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void resetToolState()
	{
		m_prevPlayerRotation = Root.instance.playerHeadGO.transform.rotation;
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		Root.instance.alignmentManager.align(oldSelection);
	}

}