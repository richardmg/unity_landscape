using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public float lodDistance1 = 100;
	public float lodDistanceCulled = 100000;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	public Lod currentLod = kLod0;

	public VoxelObject(int atlasIndex, float voxelDepth)
	{
		this.atlasIndex = atlasIndex;
		this.voxelDepth = voxelDepth;
	}

	void OnValidate()
	{
		rebuildObject();
	}

	void Start()
	{
		currentLod = kNoLod;
		Update();
	}
	
	void Update()
	{
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? kLod0 : d < lodDistanceCulled ? kLod1 : kNoLod;

		if (lod != currentLod)
			setLod(lod);
	}

	public void setLod(Lod lod)
	{
		currentLod = lod;
		rebuildObject();
	}

	public void rebuildObject()
	{
		// Don't modify the prefab itself
		if (gameObject.scene.name == null)
			return;

		VoxelMeshFactory factory = gameObject.GetComponent<VoxelMeshFactory>();
		if (factory == null) {
			if (currentLod == kNoLod)
				return;
			factory = gameObject.AddComponent<VoxelMeshFactory>();
		}

		factory.atlasIndex = atlasIndex;
		factory.voxelDepth = voxelDepth;
		factory.xFaces = voxelDepth != 0;
		factory.yFaces = voxelDepth != 0;

		switch (currentLod) {
		case kLod0:
			factory.useVolume = voxelDepth == 0;
			factory.simplify = false;
			break;
		case kLod1:
			factory.useVolume = true;
			factory.simplify = true;
			break;
		case kNoLod:
		default:
			// TODO: toggle visibility?
			return;
		}

		factory.rebuildObject();
	}
}
