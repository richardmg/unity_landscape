using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObject : MonoBehaviour {

	public int atlasIndex = 0;

	[Range (0f, 20f)]
	public float voxelDepth = 4;

	static public Material materialExact;
	static public Material materialVolume;
	static VoxelMeshFactory voxelMeshFactory;

	public static int voxelObjectCount = 0;

	public void Start()
	{
		Debug.Assert(false, "Don't add VoxelObjects (" + name + ") directly to scene. Use EntityClass/Instance instead");
	}

	void Awake()
	{
		voxelObjectCount++;
	}

	public Mesh createMesh(Lod lod)
	{
		voxelMeshFactory.atlasIndex = atlasIndex;
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
			break;
		}

		return voxelMeshFactory.createMesh();
	}

	// **************************** editor code ************************

	void OnValidate()
	{
		if (gameObject.scene.name == null || !gameObject.activeSelf) {
			// Don't modify prefabs or inactive objects
			return;
		}

		if (materialExact == null)
			initStaticResources();

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = createMesh(Root.kLod0);

		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (!meshRenderer)
			meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = materialExact;
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
	}
}
