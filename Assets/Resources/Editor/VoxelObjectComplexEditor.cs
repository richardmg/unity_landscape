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

		if(GUILayout.Button("Center and rebuild"))
		{
			vo.centerChildren();
			vo.rebuildObject();
		}

		if(GUILayout.Button("Clear"))
		{
			vo.centerChildren();
		}
	}
}
