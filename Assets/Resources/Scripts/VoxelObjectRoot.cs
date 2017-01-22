using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObjectRootMonoBehaviour : MonoBehaviour
{
	[HideInInspector]
	public VoxelObjectRoot voxelObjectRoot;
}

public class VoxelObjectRoot
{
	public Vector3 snapshotOffset = new Vector3(0, 0, -15);
	public Vector3 scale = new Vector3(1, 1, 1);

	private List<VoxelObject> voxelObjects = new List<VoxelObject>();

	public void add(VoxelObject vo)
	{
		voxelObjects.Add(vo);
		vo.voxelObjectRoot = this;
	}

	public GameObject createCombinedGameObject(Transform parent, Lod lod, string name = "VoxelObjectRoot")
	{
		GameObject go = createGameObject(parent, lod);
		go.addMeshComponents(lod, go.createCombinedMesh(lod));
		while (go.transform.childCount > 0)
			go.transform.GetChild(0).gameObject.hideAndDestroy();
		return go;
	}

	public GameObject createGameObject(Transform parent, Lod lod, string name = "VoxelObjectRoot")
	{
		GameObject go = new GameObject(name);
		foreach (VoxelObject vo in voxelObjects)
			vo.createGameObject(go.transform, lod, false);

		go.AddComponent<VoxelObjectRootMonoBehaviour>().voxelObjectRoot = this;
		go.transform.parent = parent;
		Vector3 localScale = Vector3.one;
		localScale.Scale(scale);
		localScale.Scale(Root.instance.worldScaleManager.baseScale);
		go.transform.localScale = localScale;
		go.transform.localPosition = Vector3.zero;

		return go;
	}
}
