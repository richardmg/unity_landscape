using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlignmentManager : MonoBehaviour
{
	public Vector3 voxelSize = new Vector3(0.2f, 0.2f, 0.2f);
	public float rotationStep = 22.5f;
	Transform m_childTransform;

	public void Start()
	{
		gameObject.SetActive(false);
		m_childTransform = new GameObject().transform;
		m_childTransform.parent = transform;
	}

	public float align(float v, float unit)
	{
		return Mathf.Round(v / unit) * unit;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x, voxelSize.x), align(v.y, voxelSize.y), align(v.z, voxelSize.z));
	}

	public void align(ref Vector3 position, ref Vector3 rotation)
	{
		rotation.x = align(rotation.x, rotationStep);
		rotation.y = align(rotation.y, rotationStep);
		rotation.z = align(rotation.z, rotationStep);

		transform.rotation = Quaternion.Euler(rotation);
		m_childTransform.position = position;
		m_childTransform.localPosition = align(m_childTransform.localPosition);
		position = m_childTransform.position;
	}

	public void align(Transform targetTransform)
	{
		Vector3 position = targetTransform.position;
		Vector3 rotation = targetTransform.rotation.eulerAngles;
		align(ref position, ref rotation);
		targetTransform.position = position;
		targetTransform.rotation = Quaternion.Euler(rotation);
	}

	public void align(List<EntityInstanceDescription> selection)
	{
		foreach (EntityInstanceDescription desc in selection) {
			align(ref desc.worldPos, ref desc.voxelRotation);
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

}
