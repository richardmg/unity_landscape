using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LandscapeConstructor))]
public class LandscapeConstructorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		LandscapeConstructor myScript = (LandscapeConstructor)target;
		if(GUILayout.Button("Build landscape"))
		{
			myScript.rebuildLandscape();
		}
	}
}
