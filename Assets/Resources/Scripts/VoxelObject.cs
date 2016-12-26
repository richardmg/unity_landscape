using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {

	public int atlasIndex = 0;

	[Range (0f, 20f)]
	public float voxelDepth = 4;

	static VoxelMeshFactory voxelMeshFactory;

	void OnValidate()
	{
		if (gameObject.scene.name == null || !gameObject.activeSelf) {
			// Don't modify prefabs or inactive objects
			return;
		}

		makeStandalone(Root.kLod0);
	}

	public void makeStandalone(Lod lod)
	{
		gameObject.addMeshComponents(lod, createMesh(lod));
	}

	public Mesh createMesh(Lod lod)
	{
		if (voxelMeshFactory == null) 
			voxelMeshFactory = new VoxelMeshFactory();

		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (lod) {
		case Root.kLod0:
		case Root.kLodLit:
			voxelMeshFactory.useVolume = false;
			voxelMeshFactory.simplify = false;
			break;
		case Root.kLod1:
			voxelMeshFactory.useVolume = true;
			voxelMeshFactory.simplify = true;
			break;
		case Root.kNoLod:
			break;
		}

		return voxelMeshFactory.createMesh();
	}
}
