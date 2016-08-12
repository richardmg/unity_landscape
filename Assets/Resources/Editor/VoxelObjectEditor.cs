using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObject))]
public class VoxelObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObject vo = (VoxelObject)target;
		if(GUILayout.Button("Lod 0"))
		{
			vo.setLod(VoxelObject.kLod0);
		}

		if(GUILayout.Button("Lod 1"))
		{
			vo.setLod(VoxelObject.kLod1);
		}

		if(GUILayout.Button("Center children"))
		{
			vo.centerChildren();
		}

		if(GUILayout.Button("Clear"))
		{
			vo.clear(true, true);
		}
	}
}
