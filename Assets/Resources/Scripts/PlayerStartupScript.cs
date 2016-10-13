using UnityEngine;
using System.Collections;

public class PlayerStartupScript : MonoBehaviour {
	public bool moveToGround = true;

	public EntityClass currentEntityClass;

	// Use this for initialization
	void Start () {
		currentEntityClass = new EntityClass("treefilled");

		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}
}
