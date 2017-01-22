using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlignmentManager : MonoBehaviour
{
	public Vector3 voxelSize = new Vector3(0.2f, 0.2f, 0.2f);

	public float align(float v)
	{
		return Mathf.Round(v / voxelSize.x) * voxelSize.x;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x), align(v.y), align(v.z));
	}

	public void align(Transform targetTransform)
	{
		transform.rotation = targetTransform.rotation;
		Transform descParent = targetTransform.parent;
		targetTransform.SetParent(transform, true);
		targetTransform.localPosition = align(targetTransform.localPosition);;
		targetTransform.SetParent(descParent, true);
	}

	public void align(List<EntityInstanceDescription> selection)
	{
		foreach (EntityInstanceDescription desc in selection) {
			align(desc.instance.transform);
			desc.worldPos = desc.instance.transform.position;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

}
