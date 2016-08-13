using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = kNoIndex;
	public float voxelDepth = 4;
	public Lod currentLod = kLod0;
	public float lodDistance1 = 100;
	public float lodDistanceCulled = 100000;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static public Material materialExact;
	static public Material materialVolume;
	static public Material materialVolumeSimplified;
	static VoxelMeshFactory voxelMeshFactory;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	public const Lod kTopLevel = -1;
	public const Lod kNoIndex = -2;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;

	void OnValidate()
	{
		init();
		if (atlasIndex == kTopLevel)
			centerChildren();
		rebuildObject();
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

		if (!m_meshFilter)
			init();

		m_meshFilter.sharedMesh.Clear(false);

		if (atlasIndex <= kNoIndex)
			readonlyVertexCount = 0;
		else if (atlasIndex == kTopLevel)
			rebuildAndMergeChildren();
		else
			rebuildThisObjectOnly();
	}

	public void rebuildThisObjectOnly()
	{
		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (currentLod) {
		case kLod0:
			voxelMeshFactory.useVolume = false;
			voxelMeshFactory.simplify = false;
			m_meshRenderer.sharedMaterial = materialExact;
			break;
		case kLod1:
			voxelMeshFactory.useVolume = true;
			voxelMeshFactory.simplify = true;
			m_meshRenderer.sharedMaterial = materialVolumeSimplified;
			break;
		case kNoLod:
		default:
			// TODO: toggle visibility?
			return;
		}

		m_meshFilter.sharedMesh = voxelMeshFactory.createMesh();
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void rebuildAndMergeChildren()
	{
		VoxelObject[] children = GetComponentsInChildren<VoxelObject>(true);
		for (int i = 0; i < children.Length; ++i)
			if (children[i] != this) 
				children[i].setLod(currentLod);

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		Matrix4x4 parentTransform = transform.worldToLocalMatrix;

		for (int i = 0; i < meshFilters.Length; ++i) {
			MeshFilter filter = meshFilters[i];
			combine[i].mesh = filter.sharedMesh;
			combine[i].transform = parentTransform * filter.transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
		}

		m_meshFilter.sharedMesh = new Mesh();
		m_meshFilter.sharedMesh.CombineMeshes(combine);
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

		if (voxelMeshFactory == null)
			voxelMeshFactory = new VoxelMeshFactory();
	}

	public void centerChildren()
	{
		int childCount = transform.childCount;
		if (childCount == 0)
			return;

		Vector3 firstChildPos = transform.GetChild(0).localPosition;
		for (int i = 0; i < childCount; ++i)
			transform.GetChild(i).localPosition -= firstChildPos;
	}

	public void removeMeshFilter(bool includeSelf, bool recursive)
	{
		if (includeSelf) {
//			GameObject.Destroy(gameObject.GetComponent<MeshFilter>());
//			GameObject.Destroy(gameObject.GetComponent<MeshRenderer>());
			GameObject.DestroyImmediate(gameObject.GetComponent<MeshFilter>());
			GameObject.DestroyImmediate(gameObject.GetComponent<MeshRenderer>());
			m_meshFilter = null;
			m_meshRenderer = null;
			readonlyVertexCount = 0;
		}

		if (recursive) {
			VoxelObject[] children = GetComponentsInChildren<VoxelObject>(true);
			for (int i = 0; i < children.Length; ++i) {
				if (children[i] != this) 
					children[i].removeMeshFilter(true, false);
			}
		}
	}
}
