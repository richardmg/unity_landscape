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
			vo.materialVolume.CopyPropertiesFromMaterial(vo.materialExact);
			vo.materialVolumeSimplified.CopyPropertiesFromMaterial(vo.materialExact);
		}
	}
}
