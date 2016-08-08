using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObject))]
public class VoxelCubesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObject myScript = (VoxelObject)target;
		if(GUILayout.Button("Build Object"))
		{
			myScript.rebuildObject();
		}
	}
}
