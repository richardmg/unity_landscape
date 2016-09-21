using UnityEngine;
using System.Collections;

public class KeyboardControls : MonoBehaviour {
	public GameObject player;
	public string t;

	void Update () {
		if (Input.GetKeyDown(KeyCode.T)) {
			Thing thing = new Thing();
			thing.worldPos = player.transform.position;
			thing.worldPos.y = LandscapeConstructor.instance.sampleHeight(thing.worldPos);
			thing.index = t;
			LandscapeConstructor.instance.addThing(thing);
		}
	}
}
