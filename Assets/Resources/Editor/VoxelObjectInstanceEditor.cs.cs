using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelMeshFactory))]
public class VoxelObjectInstanceEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

//		VoxelMeshFactory vo = (VoxelMeshFactory)target;
//		if(GUILayout.Button("Build Object"))
//		{
//			vo.rebuildObject();
//		}
//
//		if(GUILayout.Button("Sync materials"))
//		{
//			VoxelMeshFactory.materialVolume.CopyPropertiesFromMaterial(VoxelMeshFactory.materialExact);
//			VoxelMeshFactory.materialVolumeSimplified.CopyPropertiesFromMaterial(VoxelMeshFactory.materialExact);
//		}
	}
}
