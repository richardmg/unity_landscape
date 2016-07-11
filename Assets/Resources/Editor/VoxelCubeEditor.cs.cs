using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelCubesScript))]
public class VoxelCubesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelCubesScript myScript = (VoxelCubesScript)target;
		if(GUILayout.Button("Build Object"))
		{
			myScript.rebuildObject();
		}
	}
}
