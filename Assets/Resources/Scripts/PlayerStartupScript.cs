using UnityEngine;
using System.Collections;

public class PlayerStartupScript : MonoBehaviour {
	public bool moveToGround = true;

	public PrefabVariant currentPrefabVariant = new PrefabVariant("GameObject");

	// Use this for initialization
	void Start () {
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}
}
