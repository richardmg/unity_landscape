using UnityEngine;
using System.Collections;

public class KeyboardControls : MonoBehaviour {
	public GameObject player;
	public string t;
	public string c;

	void Update () {
		if (!Input.anyKeyDown)
			return;

		if (Input.GetKeyDown(KeyCode.T)) {
			Thing thing = new Thing();
			thing.worldPos = player.transform.position;
			thing.worldPos.y = LandscapeConstructor.instance.sampleHeight(thing.worldPos);
			thing.index = t;
			LandscapeConstructor.instance.addThing(thing);
		} else if (Input.GetKeyDown(KeyCode.C)) {
			Thing thing = new Thing();
			thing.worldPos = player.transform.position;
			thing.worldPos.y = LandscapeConstructor.instance.sampleHeight(thing.worldPos);
			thing.index = c;
			LandscapeConstructor.instance.addThing(thing);
		}
	}
}
