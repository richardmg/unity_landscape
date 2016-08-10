using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public float lodDistance1 = 100;
	public float lodDistanceCulled = 100000;

	const Lod kNoLod = -1;
	const Lod kLod0 = 0;
	const Lod kLod1 = 1;

	public Lod currentLod = kLod0;

	public VoxelObject(int atlasIndex, float voxelDepth)
	{
		this.atlasIndex = atlasIndex;
		this.voxelDepth = voxelDepth;
	}

	void Start () {
		currentLod = kNoLod;
		Update();
	}
	
	void Update () {
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? kLod0 : d < lodDistanceCulled ? kLod1 : kNoLod;
		if (lod != currentLod) {
			currentLod = lod;
			rebuildObject();
		}
	}

	void OnValidate()
	{
		rebuildObject();
	}

	public void rebuildObject()
	{
		if (gameObject.scene.name == null) {
			// Don't modify the prefab itself
			return;
		}

		VoxelObjectInstance instance = gameObject.GetComponent<VoxelObjectInstance>();
		if (instance == null) {
			if (currentLod == kNoLod)
				return;
			instance = gameObject.AddComponent<VoxelObjectInstance>();
		}

		instance.atlasIndex = atlasIndex;
		instance.voxelDepth = voxelDepth;

		switch (currentLod) {
		case kLod0:
			instance.useVolume = false;
			instance.simplify = false;
			break;
		case kLod1:
			instance.useVolume = true;
			instance.simplify = true;
			break;
		case kNoLod:
		default:
			// TODO: toggle visibility?
			return;
		}

		instance.rebuildObject();
	}
}
