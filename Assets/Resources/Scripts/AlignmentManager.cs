using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AlignmentManager : MonoBehaviour
{
	public Vector3 voxelSize = new Vector3(0.2f, 0.2f, 0.2f);
	public Vector3 rotationSteps = new Vector3(22.5f, 22.5f, 22.5f);

	public float align(float v, float unit)
	{
		return Mathf.Round(v / unit) * unit;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x, voxelSize.x), align(v.y, voxelSize.y), align(v.z, voxelSize.z));
	}

	public Quaternion align(Quaternion rotation)
	{
		Vector3 angles = rotation.eulerAngles;
		angles.x = align(angles.x, rotationSteps.x);
		angles.y = align(angles.y, rotationSteps.y);
		angles.z = align(angles.z, rotationSteps.z);
		return Quaternion.Euler(angles);
	}

	public void align(Transform targetTransform)
	{
		//targetTransform.rotation = align(targetTransform.rotation);

		Vector3 result = new Vector3();
		Vector3 angles = targetTransform.rotation.eulerAngles;
		result.x = align(angles.x, rotationSteps.x);
		result.y = align(angles.y, rotationSteps.y);
		result.z = align(angles.z, rotationSteps.z);

		float angleTowardsGround = Vector3.Angle(Vector3.up, targetTransform.up);
		float angleAroundY = Vector3.Angle(Vector3.right, targetTransform.right);
		float angleAroundZ = Quaternion.Angle(targetTransform.rotation, Quaternion.Euler(targetTransform.up));

		print("ground: " + angleTowardsGround + ", y: " + angleAroundY + ", z: " + angleAroundZ);
		//print(angles.z + ", " + result.z + ", " + targetTransform.localRotation.z);
//		targetTransform.rotation = Quaternion.Euler(0, 0, 0);
//		targetTransform.Rotate(new Vector3(0, 0, result.z), Space.Self);

		transform.rotation = targetTransform.rotation;
		Transform descParent = targetTransform.parent;
//		targetTransform.SetParent(transform, true);
//		targetTransform.localPosition = align(targetTransform.localPosition);
//		targetTransform.SetParent(descParent, true);
	}

	public void align(List<EntityInstanceDescription> selection)
	{
		foreach (EntityInstanceDescription desc in selection) {
			align(desc.instance.transform);
			desc.worldPos = desc.instance.transform.position;
			desc.rotation = desc.instance.transform.rotation;
			Root.instance.notificationManager.notifyEntityInstanceDescriptionChanged(desc);
		}
	}

}
