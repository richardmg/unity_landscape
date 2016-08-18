using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = kNoIndex;
	public float voxelDepth = 4;
	public Lod currentLod = kLod0;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static public Material materialExact;
	static public Material materialVolume;
	static VoxelMeshFactory voxelMeshFactory;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	public const Lod kTopLevel = -1;
	public const Lod kNoIndex = -2;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;

	const float lodDistance1 = 200;
	const float lodDistanceCulled = 100000;

	void OnValidate()
	{
		init();
		rebuild();
	}

	void Start()
	{
		init();
		currentLod = kNoLod;
		Update();
	}

	void Update()
	{
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? kLod0 : d < lodDistanceCulled ? kLod1 : kNoLod;

		if (lod != currentLod) {
			setLod(lod);
			rebuild();
		}
	}

	public void setLod(Lod lod)
	{
		currentLod = lod;
	}

	public void rebuild()
	{
		if (atlasIndex == kTopLevel)
			rebuildTopLevel();
		else
			rebuildObject();
	}

	public void rebuildObject()
	{
		// Don't modify the prefab itself
		if (gameObject.scene.name == null)
			return;

		if (!m_meshFilter)
			init();

		clearMesh();
		configureFactory();

		if (atlasIndex < 0)
			readonlyVertexCount = 0;
		else
			m_meshFilter.sharedMesh = voxelMeshFactory.createMesh();

		m_meshRenderer.sharedMaterial = voxelMeshFactory.useVolume ? materialVolume : materialExact;
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void rebuildTopLevel()
	{
		// Don't modify the prefab itself
		if (gameObject.scene.name == null)
			return;

		if (!m_meshFilter)
			init();

		clearMesh();
		configureFactory();

		VoxelObject[] children = GetComponentsInChildren<VoxelObject>(true);
		for (int i = 0; i < children.Length; ++i) {
			if (children[i] != this) {
				children[i].setLod(currentLod);
				children[i].rebuildObject();
			}
		}

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		Matrix4x4 parentTransform = transform.worldToLocalMatrix;

		for (int i = 0; i < meshFilters.Length; ++i) {
			MeshFilter filter = meshFilters[i];
			combine[i].mesh = filter.sharedMesh;
			combine[i].transform = parentTransform * filter.transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
		}

		gameObject.SetActive(true);

		m_meshFilter.sharedMesh = new Mesh();
		m_meshFilter.sharedMesh.CombineMeshes(combine);
		m_meshRenderer.sharedMaterial = voxelMeshFactory.useVolume ? materialVolume : materialExact;
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void configureFactory()
	{
		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (currentLod) {
		case kLod0:
			voxelMeshFactory.useVolume = false;
			voxelMeshFactory.simplify = false;
			break;
		case kLod1:
			voxelMeshFactory.useVolume = true;
			voxelMeshFactory.simplify = true;
			break;
		case kNoLod:
		default:
			// TODO: toggle visibility?
			return;
		}
	}

	public void init()
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

		if (materialExact == null) {
			materialExact = (Material)Resources.Load("Materials/VoxelObjectExact", typeof(Material));
			materialVolume = (Material)Resources.Load("Materials/VoxelObjectVolume", typeof(Material));

			Debug.Assert(materialExact != null);
			Debug.Assert(materialVolume != null);
			Debug.Assert(materialExact.mainTexture != null);
			Debug.Assert(materialVolume.mainTexture != null);

			materialVolume.CopyPropertiesFromMaterial(materialExact);
		}

		if (voxelMeshFactory == null)
			voxelMeshFactory = new VoxelMeshFactory();
	}

	public void clearMesh()
	{
		m_meshFilter.sharedMesh.Clear(false);
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}
}
