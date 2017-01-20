using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldScaleManager : MonoBehaviour
{
	public Vector3 entityBaseScale = new Vector3(0.2f, 0.2f, 0.2f);

	public void align(Transform targetTransform)
	{
		transform.rotation = targetTransform.rotation;
		Transform descParent = targetTransform.parent;
		targetTransform.SetParent(transform, true);
		Vector3 localPos = targetTransform.localPosition;
		align(ref localPos);
		targetTransform.localPosition = localPos;
		targetTransform.SetParent(descParent, true);
	}

	public float align(float v)
	{
		return Mathf.Round(v / entityBaseScale.x) * entityBaseScale.x;
	}

	public void align(ref Vector3 v)
	{
		v.x = align(v.x);
		v.y = align(v.y);
		v.z = align(v.z);
	}

}
