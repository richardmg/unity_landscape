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

		if(GUILayout.Button("Merge"))
		{
			vo.centerChildren();
			vo.rebuildObject();

			VoxelObject[] children = vo.GetComponentsInChildren<VoxelObject>(true);
			for (int i = 0; i < children.Length; ++i) {
				if (children[i] != vo)
					children[i].clear();
			}
		}

		if(GUILayout.Button("Clear"))
		{
			vo.clear();
		}
	}
}
