using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlignmentManager : MonoBehaviour
{
	public Vector3 voxelSize = new Vector3(0.2f, 0.2f, 0.2f);
	public float rotationStep = 22.5f;

	public float align(float v, float unit)
	{
		return Mathf.Floor(v / unit) * unit;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x, voxelSize.x), align(v.y, voxelSize.y), align(v.z, voxelSize.z));
	}

	public void align(Transform targetTransform)
	{
		transform.rotation = targetTransform.rotation;
		Transform descParent = targetTransform.parent;
		targetTransform.SetParent(transform, true);
		targetTransform.localPosition = align(targetTransform.localPosition);
		targetTransform.SetParent(descParent, true);
	}

	public void align(List<EntityInstanceDescription> selection)
	{
		foreach (EntityInstanceDescription desc in selection) {
//			align(desc.instance.transform);
//			desc.worldPos = desc.instance.transform.position;
//			desc.rotation = desc.instance.transform.rotation;
//			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

}
