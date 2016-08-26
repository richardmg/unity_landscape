using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LandscapeConstructor))]
public class LandscapeConstructorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

//		LandscapeConstructor lc = (LandscapeConstructor)target;
//		if (GUILayout.Button("Build landscape")) {
////			lc.rebuildLandscape();
//		}

//		if (GUILayout.Button("Clear landscape")) {
//			lc.clear();
//		}
//
//		if (GUILayout.Button("Update landscape")) {
//			Vector3	originalPos = lc.player.transform.position;
//			lc.player.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
//			lc.Update();
//			lc.player.transform.position = originalPos;
//		}
	}
}
