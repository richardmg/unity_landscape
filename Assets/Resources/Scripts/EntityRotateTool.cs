using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityRotateTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	int m_pushDirectionZ;
	int m_tippedBack;

	public void OnEnable()
	{
		onSelectionChanged(Root.instance.player.selectedEntityInstances, Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.addEntitySelectionListener(this);
		if (Root.instance.player.selectedEntityInstances.Count != 0)
			updatePushDirection();
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

		updateRotate();

		if (Root.instance.entityToolManager.playerIdle()) {
			Root.instance.alignmentManager.align(Root.instance.player.selectedEntityInstances);
			updatePushDirection();
		}
	}

	void updatePushDirection()
	{
		Transform playerTransform = Root.instance.playerGO.transform;
		Transform firstTransform = Root.instance.player.selectedEntityInstances[0].instance.transform;
		m_pushDirectionZ = (int)Root.instance.playerGO.transform.getVoxelPushDirection(firstTransform, false, false, true, Space.Self).z;
		m_tippedBack = Vector3.Dot(playerTransform.forward, firstTransform.up) < 0 ? 1 : -1;
	}

	void updateRotate()
	{
		Vector2 playerMovement = Root.instance.entityToolManager.getPlayerMovement();

		Transform firstTransform = Root.instance.player.selectedEntityInstances[0].instance.transform;
		bool straightUp = Mathf.RoundToInt(Vector3.Dot(Vector3.up, firstTransform.up) * 1000) > 995;

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.voxelRotation.x += (m_pushDirectionZ > 0 ? playerMovement.y : -playerMovement.y) * 40;
			desc.voxelRotation.y += (straightUp ? playerMovement.x : playerMovement.x * m_tippedBack) * 40;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
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