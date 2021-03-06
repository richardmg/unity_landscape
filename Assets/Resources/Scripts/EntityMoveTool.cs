﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
{

	public void OnEnable()
	{
		Root.instance.uiManager.grabMouse(this);
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
			Root.instance.entityToolManager.selectionTool.selectSingleObjectUnderPointer();
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;

		updateMove();

		if (Root.instance.entityToolManager.playerIdle())
			Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
	}

	void updateMove()
	{
		Transform firstTransform = Root.instance.player.selectedEntityInstances[0].instance.transform;
		bool flat = Mathf.RoundToInt(firstTransform.up.y * 1000) == 0;

		Vector3 headMovement = Root.instance.entityToolManager.getPlayerHeadMovement();
		Vector2 playerMovement = Root.instance.entityToolManager.getPlayerMovement();
		Vector3 pushDirection =  Root.instance.playerGO.transform.getVoxelPushDirection(firstTransform, true, true, true, Space.World);

		pushDirection.y = 0;
		pushDirection.Normalize();

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			Transform t = desc.instance.transform;
			t.Translate(Vector3.Cross(pushDirection, Vector3.up) * playerMovement.x, Space.World);
			t.Translate(pushDirection * playerMovement.y, Space.World);
			t.Translate(new Vector3(0, headMovement.y, 0), flat ? Space.World : Space.Self);

			desc.worldPos = t.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc, EntityInstanceDescription.DirtyFlags.Transform);
		}
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		Root.instance.alignmentManager.align(oldSelection);
		if (newSelection.Count != 0)
			Root.instance.player.setWalkSpeed(1);
		else
			Root.instance.player.setDefaultWalkSpeed();
	}

}