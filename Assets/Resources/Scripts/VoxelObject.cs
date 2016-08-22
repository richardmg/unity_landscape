using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = kNoIndex;
	public float voxelDepth = 4;
	public Lod currentLod = kLod0;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;
	bool meshComponentsInitialized = false;

	static bool staticResourcesInitialized = false;
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
		if (gameObject.scene.name == null) {
			// Don't modify prefabs
			readonlyVertexCount = 0;
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
		m_meshFilter.sharedMesh = createTopLevelMesh(currentLod);
		m_meshRenderer.sharedMaterial = (currentLod == kLod0) ? materialExact : materialVolume;
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void setChildrenActive(bool active)
	{
		bool isActive = gameObject.activeSelf;
		GameObject[] selfAndchildren = GetComponentsInChildren<GameObject>(true);
		for (int i = 0; i < selfAndchildren.Length; ++i)
			selfAndchildren[i].SetActive(active);
		if (isActive)
			gameObject.SetActive(true);
	}

	public Mesh createMesh(Lod lod)
	{
		if (atlasIndex < 0)
			return new Mesh();

		configureFactory(lod);
		return voxelMeshFactory.createMesh();
	}

	public Mesh createTopLevelMesh(Lod lod)
	{
		VoxelObject[] selfAndchildren = GetComponentsInChildren<VoxelObject>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			VoxelObject vo = selfAndchildren[i];
//			vo.ensureInitialized();
			vo.setLod(currentLod);
			combine[i].mesh = vo.createMesh(lod);
			combine[i].transform = parentTransform * vo.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);
		return topLevelMesh;
	}

	public void configureFactory(Lod lod)
	{
		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (lod) {
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

	public void initMeshComponents()
	{
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
		voxelMeshFactory = new VoxelMeshFactory();

		staticResourcesInitialized = true;
	}
}
