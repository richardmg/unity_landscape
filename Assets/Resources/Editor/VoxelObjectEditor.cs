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

		if (!vo.isTopLevel() && GUILayout.Button("Make top level")) {
			vo.setTopLevel(true);
			vo.rebuildStandAlone();
		}

		if (vo.isTopLevel() && GUILayout.Button("Undo top level")) {
			vo.setTopLevel(false);
			vo.rebuildStandAlone();
		}
	}
}
