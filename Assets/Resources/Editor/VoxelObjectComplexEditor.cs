using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObjectComplex))]
public class VoxelObjectComplexEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObjectComplex vo = (VoxelObjectComplex)target;
		if(GUILayout.Button("Lod 0"))
		{
			vo.setLod(VoxelObjectComplex.kLod0);
		}

		if(GUILayout.Button("Lod 1"))
		{
			vo.setLod(VoxelObjectComplex.kLod1);
		}

		if(GUILayout.Button("Rebuild"))
		{
			vo.rebuildObject();
		}
	}
}
