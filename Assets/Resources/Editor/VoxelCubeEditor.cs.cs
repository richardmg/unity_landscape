using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObject))]
public class VoxelCubesEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObject vo = (VoxelObject)target;
		if(GUILayout.Button("Build Object"))
		{
			vo.rebuildObject();
		}

		if(GUILayout.Button("Sync materials"))
		{
			VoxelObject.materialVolume.CopyPropertiesFromMaterial(VoxelObject.materialExact);
			VoxelObject.materialVolumeSimplified.CopyPropertiesFromMaterial(VoxelObject.materialExact);
		}
	}
}
