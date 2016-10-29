using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObjectRoot : MonoBehaviour {

	public Vector3 snapshotOffset = new Vector3(0, 0, -7);

	public Mesh createMesh(Lod lod, Dictionary<int, int> indexSubstitutions)
	{
		VoxelObject[] selfAndchildren = GetComponentsInChildren<VoxelObject>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			VoxelObject vo = selfAndchildren[i];
			combine[i].mesh = vo.createMesh(lod, indexSubstitutions);
			combine[i].transform = parentTransform * vo.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
