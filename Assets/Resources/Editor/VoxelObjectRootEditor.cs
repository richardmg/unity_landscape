using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(VoxelObjectRoot))]
public class VoxelObjectEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VoxelObjectRoot voRoot = (VoxelObjectRoot)target;
		if (GUILayout.Button("Rebuild")) {
			VoxelObject[] selfAndchildren = voRoot.gameObject.GetComponentsInChildren<VoxelObject>(true);
			for (int i = 0; i < selfAndchildren.Length; ++i)
				selfAndchildren[i].makeStandalone(Root.kLod0);
		}
	}
}
