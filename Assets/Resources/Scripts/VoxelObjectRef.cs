using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObjectRef : MonoBehaviour {
	public string prefabName;
	public Lod currentLod = VoxelObject.kLod0;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;
	bool meshComponentsInitialized = false;

	static bool staticResourcesInitialized = false;
	static public Material materialExact;
	static public Material materialVolume;

	// Read-only, for editor inspection
	public int vertexCount = 0;

	const float lodDistance1 = 200;
	const float lodDistanceCulled = 100000;

	void OnValidate()
	{
		if (gameObject.scene.name == null) {
			// Don't create and store mesh for prefabs
			vertexCount = 0;
			return;
		}

		if (!gameObject.activeSelf)
			return;

		ensureInitialized();
		rebuild();
	}

	void Start()
	{
		ensureInitialized();
		currentLod = VoxelObject.kNoLod;
		Update();
	}

	void Update()
	{
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? VoxelObject.kLod0 : d < lodDistanceCulled ? VoxelObject.kLod1 : VoxelObject.kNoLod;

		if (lod != currentLod) {
			setLod(lod);
			rebuild();
		}
	}

	public void ensureInitialized()
	{
		if (!staticResourcesInitialized)
			initStaticResources();
		if (!meshComponentsInitialized)
			initMeshComponents();
	}

	public void setLod(Lod lod)
	{
		currentLod = lod;
	}

	public void rebuild()
	{
		m_meshFilter.sharedMesh = VoxelObjectCache.instance().getSharedMesh(prefabName, currentLod);
		if (m_meshFilter.sharedMesh == null)
			return;

		m_meshRenderer.sharedMaterial = (currentLod == VoxelObject.kLod0) ? materialExact : materialVolume;
		vertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void initMeshComponents()
	{
		if (gameObject.scene.name == null)
			return;

		if (!m_meshFilter) {
			m_meshFilter = gameObject.GetComponent<MeshFilter>();
			if (!m_meshFilter)
				m_meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		}

		if (!m_meshFilter.sharedMesh)
			m_meshFilter.sharedMesh = new Mesh();

		if (!m_meshRenderer) {
			m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (!m_meshRenderer)
				m_meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		}

		meshComponentsInitialized = true;
	}

	public void initStaticResources()
	{
		materialExact = (Material)Resources.Load("Materials/VoxelObjectExact", typeof(Material));
		materialVolume = (Material)Resources.Load("Materials/VoxelObjectVolume", typeof(Material));

		Debug.Assert(materialExact != null);
		Debug.Assert(materialVolume != null);
		Debug.Assert(materialExact.mainTexture != null);
		Debug.Assert(materialVolume.mainTexture != null);

		materialVolume.CopyPropertiesFromMaterial(materialExact);
		staticResourcesInitialized = true;
	}
}
