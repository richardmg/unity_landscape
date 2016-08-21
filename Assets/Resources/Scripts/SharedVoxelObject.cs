using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class SharedVoxelObject : MonoBehaviour {
	public GameObject voxelObject;

	private GameObject lod0;
	private GameObject lod1;

	void OnValidate()
	{
		gameObject.SetActive(false);
	}

	void Start()
	{
		gameObject.SetActive(false);
	}

	public GameObject getVoxelObject(Lod lod)
	{
		switch(lod) {
		case VoxelObject.kLod0:
			if (lod0 == null) {
				lod0 = GameObject.Instantiate(voxelObject);
				lod0.transform.SetParent(transform);
				lod0.SetActive(false);
				VoxelObject vo = lod0.GetComponent<VoxelObject>();
				vo.setLod(VoxelObject.kLod0);
				vo.rebuild();
			}
			return lod0;
		default:
			if (lod1 == null) {
				lod1 = GameObject.Instantiate(voxelObject);
				lod1.transform.SetParent(transform);
				lod1.SetActive(false);
				VoxelObject vo = lod0.GetComponent<VoxelObject>();
				vo.setLod(VoxelObject.kLod1);
				vo.rebuild();
			}
			return lod1;
		}
	}
}
