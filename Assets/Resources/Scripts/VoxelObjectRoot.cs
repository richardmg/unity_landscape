using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObjectRoot
{
	public List<VoxelObject> m_voxelObjects = new List<VoxelObject>();
	public Vector3 snapshotOffset = new Vector3(0, 0, -7);

	public Vector3 scale = new Vector3(1, 1, 1);

	public Mesh createMesh(Lod lod)
	{
		return new Mesh();

//		CombineInstance[] combine = new CombineInstance[m_voxelObjects.Count];
//		Matrix4x4 parentTransform = transform.worldToLocalMatrix;
//
//		for (int i = 0; i < m_voxelObjects.Count; ++i) {
//			VoxelObject vo = m_voxelObjects[i];
//			combine[i].mesh = vo.createMesh(lod);
//			combine[i].transform = parentTransform * vo.transform.localToWorldMatrix;
//		}
//
//		Mesh topLevelMesh = new Mesh();
//		topLevelMesh.CombineMeshes(combine);
//
//		return topLevelMesh;
	}

	public GameObject createGameObject(Transform parent, Lod lod)
	{
		GameObject go = new GameObject("VoxelObjectRoot");
		go.transform.parent = parent;
		go.transform.localScale = scale;
		go.transform.localPosition = Vector3.zero;
		foreach (VoxelObject vo in m_voxelObjects)
			vo.createGameObject(go.transform, lod);
		return go;
	}
}
