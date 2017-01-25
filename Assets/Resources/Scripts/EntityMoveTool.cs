using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using ToolMode = System.Int32;

public class EntityMoveTool : MonoBehaviour, IEntityInstanceSelectionListener
{
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

		updateMove();
		Root.instance.entityToolManager.updateAlignment();
	}

	void updateMove()
	{
		Vector3 headPos = Root.instance.playerHeadGO.transform.position;
		Vector3 headDir = Root.instance.playerHeadGO.transform.forward;

		Vector3 normalizedHeadPos = headPos - m_lastHeadPos;
		Vector3 ortogonalHeadDir = Vector3.Cross(m_lastHeadDirection, Vector3.up);
		float zMovement = Vector3.Dot(normalizedHeadPos, m_lastHeadDirection);
		float xMovement = Vector3.Dot(normalizedHeadPos, ortogonalHeadDir) * -1;

		m_lastHeadPos = headPos;
		m_lastHeadDirection = headDir;

		// Calculate how much the head has tilted up/down and left/right
		Quaternion playerRotation = Root.instance.playerHeadGO.transform.rotation;
		float yMovement = Mathf.DeltaAngle(playerRotation.eulerAngles.x, m_prevPlayerRotation.eulerAngles.x) * 0.05f;
		m_prevPlayerRotation = playerRotation;

		// Inform the app about the position update of the selected objects
		foreach (EntityInstanceDescription desc in Root.instance.player.selectedEntityInstances) {
			Transform t = desc.instance.transform;
			Vector3 right = t.right;
			right.y = 0;
			right.Normalize();

			Vector3 forward = t.forward;
			if (forward.y == 0)
				forward.y = 1;
			else if (forward.y < 0)
				zMovement *= -1;
			else
				forward.y = 0;
			forward.Normalize();

			t.Translate(right * xMovement, Space.World);
			t.Translate(forward * zMovement, Space.World);
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