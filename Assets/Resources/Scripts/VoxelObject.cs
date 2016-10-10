using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	public static int voxelObjectCount = 0;

	public const Lod kTopLevel = -1;
	public const Lod kPrefab = -2;
	public const Lod kEmpty = -3;
	public const Lod kUnknown = -4;

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
		case kPrefab: return "prefab";
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
			return;
		}

		resolveAtlasIndex();
		initMeshComponents();

		if (!staticResourcesInitialized)
			initStaticResources();

		m_meshFilter.sharedMesh = MeshManager.createMeshFromAtlasIndex(resolvedIndex(), Root.kLod0, voxelDepth);
		print("size: " + m_meshFilter.sharedMesh.vertexCount);
		m_meshRenderer.sharedMaterial = materialExact;
	}

	public void resolveAtlasIndex()
	{
		// Remove resolvedIndex indirection once everything works again

		// if index is a number >= 0, then it's an index to a sub image in the texture atlas.
		if (!System.Int32.TryParse(index, out m_resolvedIndex)) {
			if (index == indexToString(kTopLevel))
				m_resolvedIndex = kTopLevel;
			else if (index == indexToString(kEmpty))
				m_resolvedIndex = kEmpty;
			else
				m_resolvedIndex = kPrefab;
		}
	}

	public void setChildrenActive(bool active)
	{
		Debug.Assert(false, "Refactor this so that hidden children goes into a hidden root object instead!");
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
		staticResourcesInitialized = true;
	}

	public void clearMesh()
	{
		if (!m_meshFilter)
			return;
		m_meshFilter.sharedMesh.Clear();
	}

}
