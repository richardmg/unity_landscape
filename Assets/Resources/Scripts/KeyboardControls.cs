using UnityEngine;
using System.Collections;

public class KeyboardControls : MonoBehaviour {
	public string t;

	void Update () {
		if (Input.GetKeyDown(KeyCode.T)) {
			Thing thing = new Thing();
			thing.worldPos = new Vector3(0, 0, 0);
			thing.worldPos.y = LandscapeConstructor.instance.sampleHeight(thing.worldPos);
			thing.index = t;
			LandscapeConstructor.instance.addThing(thing);
		}
	}
}
