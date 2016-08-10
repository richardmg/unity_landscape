using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObjectInstance))]
public class VoxelCubesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObjectInstance vo = (VoxelObjectInstance)target;
		if(GUILayout.Button("Build Object"))
		{
			vo.rebuildObject();
		}

		if(GUILayout.Button("Sync materials"))
		{
			VoxelObjectInstance.materialVolume.CopyPropertiesFromMaterial(VoxelObjectInstance.materialExact);
			VoxelObjectInstance.materialVolumeSimplified.CopyPropertiesFromMaterial(VoxelObjectInstance.materialExact);
		}
	}
}
