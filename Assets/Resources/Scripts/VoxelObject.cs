using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObject
{
	public int atlasIndex = 0;
	public float voxelDepth = 4;

	public Vector3 localPos;
	public Quaternion localRotation;

	static VoxelMeshFactory voxelMeshFactory;

	public VoxelObject(int atlasIndex, float voxelDepth)
	{
		this.atlasIndex = atlasIndex;
		this.voxelDepth = voxelDepth;
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

	public GameObject createGameObject(Transform parent, Lod lod)
	{
		GameObject go = new GameObject("VoxelObject: " + atlasIndex);
		go.addMeshComponents(lod, createMesh(lod));
		go.transform.parent = parent;
		go.transform.localPosition = localPos;
		go.transform.localRotation = localRotation;
		return go;
	}
}
