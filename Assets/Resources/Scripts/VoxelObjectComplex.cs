using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObjectComplex : MonoBehaviour {
	public Lod currentLod = kLod0;
	public float lodDistance1 = 100;
	public float lodDistanceCulled = 100000;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static public Material materialExact;
	static public Material materialVolume;
	static public Material materialVolumeSimplified;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;

	void OnValidate()
	{
		init();
		rebuildObject();
	}

	void Start()
	{
		currentLod = kNoLod;
		init();
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

	public void init()
	{
		if (gameObject.scene.name == null)
			return;

		m_meshFilter = gameObject.GetComponent<MeshFilter>();
		m_meshRenderer = gameObject.GetComponent<MeshRenderer>();

		if (!m_meshFilter)
			m_meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();

		if (!m_meshRenderer)
			m_meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();

		if (materialExact == null) {
			materialExact = (Material)Resources.Load("Materials/VoxelObjectExact", typeof(Material));
			materialVolume = (Material)Resources.Load("Materials/VoxelObjectVolume", typeof(Material));
			materialVolumeSimplified = (Material)Resources.Load("Materials/VoxelObjectVolumeSimplified", typeof(Material));

			Debug.Assert(materialExact != null);
			Debug.Assert(materialVolume != null);
			Debug.Assert(materialVolumeSimplified != null);
			Debug.Assert(materialExact.mainTexture != null);
			Debug.Assert(materialVolume.mainTexture != null);
			Debug.Assert(materialVolumeSimplified.mainTexture != null);

			materialVolume.CopyPropertiesFromMaterial(materialExact);
			materialVolumeSimplified.CopyPropertiesFromMaterial(materialExact);
		}

		// TODO: Change out with Color32 matrix, which should be faster access to pixels.
		// And, need to fetch texture from other place than MeshRenderer.
		VoxelMeshFactory.texture = (Texture2D)materialExact.mainTexture;
	}

	public void rebuildObject()
	{
		// Don't modify the prefab itself
		if (gameObject.scene.name == null)
			return;

		m_meshFilter.sharedMesh = new Mesh();
		Transform prevTransform = this.transform;
		transform.localRotation = Quaternion.identity;
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		for (int i = 0; i < meshFilters.Length; ++i) {
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
		}

		m_meshFilter.sharedMesh = new Mesh();
		m_meshFilter.sharedMesh.CombineMeshes(combine);
		transform.localRotation = prevTransform.localRotation;
		transform.localPosition = prevTransform.localPosition;
		transform.localScale = prevTransform.localScale;
		gameObject.SetActive(true);

		switch (currentLod) {
		case kLod0:
			m_meshRenderer.sharedMaterial = materialExact;
			break;
		case kLod1:
			m_meshRenderer.sharedMaterial = materialVolumeSimplified;
			break;
		default:
			break;
		}

		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}
}
