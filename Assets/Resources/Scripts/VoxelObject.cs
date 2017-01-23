using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class VoxelObjectMonoBehaviour : MonoBehaviour
{
	[HideInInspector] public VoxelObject voxelObject;
}

public class VoxelObject
{
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public VoxelObjectRoot voxelObjectRoot;

	public Vector3 localPosition;
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

	public GameObject createGameObject(Transform parent, Lod lod, bool applyGlobalScale)
	{
		GameObject go = new GameObject("VoxelObject: " + atlasIndex);
		go.addMeshComponents(lod, createMesh(lod));
		go.AddComponent<VoxelObjectMonoBehaviour>().voxelObject = this;
		go.transform.parent = parent;
		go.transform.localPosition = localPosition;
		go.transform.localRotation = localRotation;
		go.transform.localScale = Vector3.one;
		if (applyGlobalScale) {
			// Only apply global scale if go will exist as
			// standalone, and not as a child of a VoxelObjectRoot.
			go.transform.localScale = Root.instance.alignmentManager.voxelSize;
		}
		return go;
	}
}
