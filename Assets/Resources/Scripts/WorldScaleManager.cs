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
		Vector3 localPos = align(targetTransform.localPosition);
		targetTransform.localPosition = localPos;
		targetTransform.SetParent(descParent, true);
	}

	public float align(float v)
	{
		return Mathf.Round(v / entityBaseScale.x) * entityBaseScale.x;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x), align(v.y), align(v.z));
	}

}
