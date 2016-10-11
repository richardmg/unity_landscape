using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {

	public string index = System.String.Empty;

	[Range (0f, 20f)]
	public float voxelDepth = 4;
	[Range (0, 1)]
	public Lod currentLod = Root.kLod0;

	int m_resolvedIndex = kUnknown;
	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static bool staticResourcesInitialized = false;
	static public Material materialExact;
	static public Material materialVolume;
	static VoxelMeshFactory voxelMeshFactory;
	public static int voxelObjectCount = 0;

	public const Lod kTopLevel = -1;
	public const Lod kEmpty = -3;
	public const Lod kUnknown = -4;

	// Read-only, for editor inspection
	public int vertexCount = 0;

	const float lodDistance1 = 200;
	const float lodDistanceCulled = 100000;

	public int resolvedIndex()
	{
		Debug.Assert(m_resolvedIndex != kUnknown);
		return m_resolvedIndex;
	}

	static public string indexToString(int index)
	{
		switch(index) {
		case kTopLevel: return "toplevel";
		case kEmpty: return "empty";
		}

		return "unknown";
	}

	void Awake()
	{
		voxelObjectCount++;
	}

	void OnValidate()
	{
		resolveAtlasIndex();

		if (gameObject.scene.name == null || !gameObject.activeSelf) {
			// Don't modify prefabs or inactive objects
			vertexCount = 0;
			return;
		}

		initAsStandAlone();
		rebuildStandAlone();
	}

	void Start()
	{
		resolveAtlasIndex();
		initAsStandAlone();
		currentLod = Root.kNoLod;
		Update();
	}

	void Update()
	{
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? Root.kLod0 : d < lodDistanceCulled ? Root.kLod1 : Root.kNoLod;

		if (lod != currentLod) {
			setLod(lod);
			rebuildStandAlone();
		}
	}

	public void initAsStandAlone()
	{
		resolveAtlasIndex();
		initMeshComponents();

		if (!staticResourcesInitialized)
			initStaticResources();
	}

	public void setIndex(string index)
	{
		this.index = index;
		resolveAtlasIndex();
	}

	public void resolveAtlasIndex()
	{
		// if index is a number >= 0, then it's an index to a sub image in the texture atlas.
		if (!System.Int32.TryParse(index, out m_resolvedIndex)) {
			if (index == indexToString(kTopLevel))
				m_resolvedIndex = kTopLevel;
			else if (index == indexToString(kEmpty))
				m_resolvedIndex = kEmpty;
		}
	}

	public void setLod(Lod lod)
	{
		currentLod = lod;
	}

	public void rebuildStandAlone()
	{
		m_meshFilter.sharedMesh = createMesh(currentLod);
		if (m_meshFilter.sharedMesh == null) {
			vertexCount = 0;
			return;
		}

		m_meshRenderer.sharedMaterial = (currentLod == Root.kLod0) ? materialExact : materialVolume;
		vertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void setChildrenActive(bool active)
	{
		bool isActive = gameObject.activeSelf;
		VoxelObject[] selfAndchildren = GetComponentsInChildren<VoxelObject>(true);
		if (active) {
			for (int i = 0; i < selfAndchildren.Length; ++i) {
				selfAndchildren[i].initAsStandAlone();
				selfAndchildren[i].rebuildStandAlone();
				selfAndchildren[i].gameObject.SetActive(true);
			}
		} else {
			for (int i = 0; i < selfAndchildren.Length; ++i)
				selfAndchildren[i].gameObject.SetActive(false);
		}
		if (isActive)
			gameObject.SetActive(true);
	}

	public void setTopLevel(bool topLevel)
	{
		if (topLevel) {
			int childCount = transform.childCount;
			if (childCount == 0)
				return;

			Vector3 firstChildPos = transform.GetChild(0).localPosition;
			for (int i = 0; i < childCount; ++i)
				transform.GetChild(i).localPosition -= firstChildPos;

			index = indexToString(kTopLevel);
			resolveAtlasIndex();
			setChildrenActive(false);
		} else {
			index = indexToString(kEmpty);
			resolveAtlasIndex();
			setChildrenActive(true);
			clearMesh();
		}
	}

	public bool isTopLevel()
	{
		return m_resolvedIndex == kTopLevel;
	}

	public Mesh createMesh(Lod lod)
	{
		return isTopLevel() ? createTopLevelMesh(currentLod) : createMeshNonRecursive(currentLod);
	}

	public Mesh createTopLevelMesh(Lod lod)
	{
		// Return a mesh that is a combination of this object and all its children

		VoxelObject[] selfAndchildren = GetComponentsInChildren<VoxelObject>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			VoxelObject vo = selfAndchildren[i];
			vo.setLod(currentLod);
			combine[i].mesh = vo.createMeshNonRecursive(lod);
			combine[i].transform = parentTransform * vo.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}

	Mesh createMeshNonRecursive(Lod lod)
	{
		// Return a mesh that represents this object only

		if (m_resolvedIndex == kTopLevel || m_resolvedIndex == kEmpty) {
			// Return empty mesh since we don't recurse
			return new Mesh();
		}

		// Invariant: m_resolvedIndex points to a sub image in the texture atlas.
		// We don't cache sub image meshes since they are not likely reused across different prefabs
		configureFactory(lod);
		return voxelMeshFactory.createMesh();
	}

	public void configureFactory(Lod lod)
	{
		voxelMeshFactory.atlasIndex = m_resolvedIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (lod) {
		case Root.kLod0:
			voxelMeshFactory.useVolume = false;
			voxelMeshFactory.simplify = false;
			break;
		case Root.kLod1:
			voxelMeshFactory.useVolume = true;
			voxelMeshFactory.simplify = true;
			break;
		case Root.kNoLod:
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
	}

	public static void initStaticResources()
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

	public void clearMesh()
	{
		if (!m_meshFilter)
			return;
		m_meshFilter.sharedMesh.Clear();
		vertexCount = 0;
	}
}
