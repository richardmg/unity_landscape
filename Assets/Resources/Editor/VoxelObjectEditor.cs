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

		if (vo.currentLod == 0 && GUILayout.Button("Use lod 1")) {
			vo.setLod(VoxelObject.kLod1);
			vo.rebuild();
		}

		if (vo.currentLod == 1 && GUILayout.Button("Use lod 0")) {
			vo.setLod(VoxelObject.kLod0);
			vo.rebuild();
		}

		if (!vo.isTopLevel() && GUILayout.Button("Make top level")) {
			vo.setTopLevel(true);
			vo.rebuild();
		}

		if (vo.isTopLevel() && GUILayout.Button("Undo top level")) {
			vo.setTopLevel(false);
			vo.rebuild();
		}
	}
}
