using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class DynamicLod : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public float lodDistance1 = 10;
	public float lodDistanceCulled = 1000;

	const Lod kNoLod = -1;
	const Lod kLod0 = 0;
	const Lod kLod1 = 1;

	private Lod currentLod = kNoLod;

	void Start () {
		Update();
	}
	
	void Update () {
		float d = cameraDistance();
		Lod lod = d < lodDistance1 ? kLod0 : d < lodDistanceCulled ? kLod1 : kNoLod;
		if (lod != currentLod) {
			currentLod = lod;
			rebuildObject();
		}
	}

	void OnValidate()
	{
		currentLod = kLod0;
		rebuildObject();
	}

	public float cameraDistance()
	{
		Transform camera = Camera.main.transform;
		Vector3 heading = transform.position - camera.position;
		return Vector3.Dot(heading, camera.forward);
	}

	public void rebuildObject()
	{
		VoxelObject voxelObject = gameObject.GetComponent<VoxelObject>();
		if (voxelObject == null) {
			if (currentLod == kNoLod)
				return;
			voxelObject = gameObject.AddComponent<VoxelObject>();
		}

		voxelObject.atlasIndex = atlasIndex;
		voxelObject.voxelDepth = voxelDepth;

		switch (currentLod) {
		case kLod0:
			voxelObject.useVolume = false;
			voxelObject.simplify = false;
			break;
		case kLod1:
			voxelObject.useVolume = true;
			voxelObject.simplify = true;
			break;
		case kNoLod:
		default:
			GameObject.DestroyImmediate(voxelObject);
			voxelObject = null;
			return;
		}

		voxelObject.rebuildObject();
	}
}
