using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TileLayerVoxelObjects))]
public class VoxelObjectInstanceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

//		TileLayerVoxelObjects vo = (TileLayerVoxelObjects)target;
//		if(GUILayout.Button("Build Object"))
//		{
//			vo.rebuildObject();
//		}
	}
}
