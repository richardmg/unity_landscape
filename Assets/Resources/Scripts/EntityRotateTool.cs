using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityRotateTool : MonoBehaviour, IEntityInstanceSelectionListener
{
	Vector3 m_pushDirection;

	public void OnEnable()
	{
		Root.instance.player.setWalkSpeed(1);
		onSelectionChanged(Root.instance.player.selectedEntityInstances, Root.instance.player.selectedEntityInstances);
		Root.instance.notificationManager.addEntitySelectionListener(this);

		Transform firstTransform = Root.instance.player.selectedEntityInstances[0].instance.transform;
		m_pushDirection = Root.instance.playerGO.transform.getVoxelPushDirection(firstTransform, false, Space.Self);
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
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;

		updateRotate();
		Root.instance.entityToolManager.updateAlignment();
	}

	void updateRotate()
	{
		Vector2 playerMovement = Root.instance.entityToolManager.getPlayerMovement();

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			desc.voxelRotation.x += (m_pushDirection.z != 0 ? (playerMovement.y * m_pushDirection.z) : (playerMovement.x * m_pushDirection.x)) * 40;
			desc.voxelRotation.y += (m_pushDirection.z != 0 ? -(playerMovement.x * m_pushDirection.z) : (playerMovement.y * m_pushDirection.x)) * 40;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

	public void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		Root.instance.alignmentManager.align(oldSelection);
	}

}