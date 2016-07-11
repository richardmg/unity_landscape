using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelCubesScript))]
public class ObjectBuilderEditor : Editor
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