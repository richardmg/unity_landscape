﻿using UnityEngine;
using System.Collections;

public class PlayerStartupScript : MonoBehaviour {
	public bool moveToGround = true;

	// Use this for initialization
	void Start () {
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = LandscapeConstructor.instance.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}
}