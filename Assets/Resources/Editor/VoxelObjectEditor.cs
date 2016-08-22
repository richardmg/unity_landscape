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

		if (vo.atlasIndex != -1 && GUILayout.Button("Make top level")) {
			Transform transform = vo.transform;
			int childCount = transform.childCount;
			if (childCount == 0)
				return;

			Vector3 firstChildPos = transform.GetChild(0).localPosition;
			for (int i = 0; i < childCount; ++i)
				transform.GetChild(i).localPosition -= firstChildPos;

			vo.atlasIndex = -1;
			vo.rebuild();
			vo.setChildrenActive(false);
		}

		if (vo.atlasIndex == -1 && GUILayout.Button("Undo top level")) {
			VoxelObject[] children = vo.GetComponentsInChildren<VoxelObject>(true);
			for (int i = 0; i < children.Length; ++i)
				children[i].gameObject.SetActive(true);

			vo.atlasIndex = -2;
			vo.rebuild();
			vo.setChildrenActive(true);
		}
	}
}
