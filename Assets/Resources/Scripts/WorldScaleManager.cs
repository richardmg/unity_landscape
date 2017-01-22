﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldScaleManager : MonoBehaviour
{
	public Vector3 baseScale = new Vector3(0.2f, 0.2f, 0.2f);

	public void align(Transform targetTransform)
	{
		transform.rotation = targetTransform.rotation;
		Transform descParent = targetTransform.parent;
		targetTransform.SetParent(transform, true);
		targetTransform.localPosition = align(targetTransform.localPosition);;
		targetTransform.SetParent(descParent, true);
	}

	public float align(float v)
	{
		return Mathf.Round(v / baseScale.x) * baseScale.x;
	}

	public Vector3 align(Vector3 v)
	{
		return new Vector3(align(v.x), align(v.y), align(v.z));
	}

}
