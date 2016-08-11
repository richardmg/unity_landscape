using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public Lod currentLod = kLod0;
	public float lodDistance1 = 100;
	public float lodDistanceCulled = 100000;

	MeshFilter m_meshFilter;
	MeshRenderer m_meshRenderer;

	static public Material materialExact;
	static public Material materialVolume;
	static public Material materialVolumeSimplified;
	static VoxelMeshFactory voxelMeshFactory = new VoxelMeshFactory();

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;

	public VoxelObject(int atlasIndex, float voxelDepth)
	{
		this.atlasIndex = atlasIndex;
		this.voxelDepth = voxelDepth;
	}

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

	public void rebuildObject()
	{
		// Don't modify the prefab itself
		if (gameObject.scene.name == null)
			return;

		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (currentLod) {
		case kLod0:
			voxelMeshFactory.useVolume = voxelDepth == 0;
			voxelMeshFactory.simplify = false;
			m_meshRenderer.sharedMaterial = voxelMeshFactory.useVolume ? materialVolume : materialExact;
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

		m_meshFilter.mesh = voxelMeshFactory.createMesh();
		readonlyVertexCount = m_meshFilter.sharedMesh.vertices.Length;
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

	public void mergeChildren()
	{
		// Not sure how to this, since merging children will make it hard to do lodding.
		// I need to store the config somehow (transform, index, depth etc) for each child.
		// And I need to be careful not to merge meshes runtime. So perhaps I can also
		// store the number of vertices needed to build all children (or perhaps just preallocate
		// a max value  array in the factory. After all it's static.) And the apply the
		// factory child by child, enabling vertex sharing etc as well.
	}
}
