﻿using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public string index = System.String.Empty;
	public float voxelDepth = 4;
	public Lod currentLod = kLod0;

	int atlasIndex = kIndexUnknown;
	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static bool staticResourcesInitialized = false;
	static public Material materialExact;
	static public Material materialVolume;
	static VoxelMeshFactory voxelMeshFactory;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	public const Lod kIndexTopLevel = -1;
	public const Lod kIndexReference = -2;
	public const Lod kIndexUnknown = -3;

	public const string kTopLevelString = "toplevel";

	// Read-only, for editor inspection
	public int vertexCount = 0;

	const float lodDistance1 = 200;
	const float lodDistanceCulled = 100000;

	void OnValidate()
	{
		if (gameObject.scene.name == null) {
			// Don't modify prefabs
			vertexCount = 0;
			return;
		}

		if (!gameObject.activeSelf)
			return;

		initVoxelObject();
		reconstructGameObject();
	}

	void Start()
	{
		initVoxelObject();
		currentLod = kNoLod;
		Update();
	}

	void Update()
	{
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		Lod lod = d < lodDistance1 ? kLod0 : d < lodDistanceCulled ? kLod1 : kNoLod;

		if (lod != currentLod) {
			setLod(lod);
			reconstructGameObject();
		}
	}

	public void initVoxelObject()
	{
		initMeshComponents();

		if (!staticResourcesInitialized)
			initStaticResources();
	}

	public void determineAtlasIndex()
	{
		if (!System.Int32.TryParse(index, out atlasIndex)) {
			if (index == kTopLevelString)
				atlasIndex = kIndexTopLevel;
			else
				atlasIndex = kIndexReference;
		}
	}

	public void setLod(Lod lod)
	{
		currentLod = lod;
	}

	public void reconstructGameObject()
	{
		m_meshFilter.sharedMesh = createTopLevelMesh(currentLod);
		if (m_meshFilter.sharedMesh == null) {
			vertexCount = 0;
			return;
		}

		m_meshRenderer.sharedMaterial = (currentLod == VoxelObject.kLod0) ? materialExact : materialVolume;
		vertexCount = m_meshFilter.sharedMesh.vertices.Length;
	}

	public void setChildrenActive(bool active)
	{
		bool isActive = gameObject.activeSelf;
		VoxelObject[] selfAndchildren = GetComponentsInChildren<VoxelObject>(true);
		for (int i = 0; i < selfAndchildren.Length; ++i)
			selfAndchildren[i].gameObject.SetActive(active);
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

			index = kTopLevelString;
			atlasIndex = kIndexTopLevel;
			setChildrenActive(false);
		} else {
			index = "0";
			atlasIndex = 0;
			setChildrenActive(true);
		}
	}

	public bool isTopLevel()
	{
		return atlasIndex == kIndexTopLevel;
	}

	public Mesh createMesh(Lod lod)
	{
//		if (atlasIndex == kIndexUnknown)
			determineAtlasIndex();
			
		if (atlasIndex == kIndexReference) {
			Mesh sharedMesh = VoxelObjectCache.instance().getSharedMesh(index, lod);
			return (sharedMesh != null) ? sharedMesh : new Mesh();
		}

		if (atlasIndex == kIndexTopLevel)
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
