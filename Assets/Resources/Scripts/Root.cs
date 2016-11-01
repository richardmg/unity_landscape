using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class Root : MonoBehaviour {
	public static readonly int kAtlasWidth = 2048;
	public static readonly int kAtlasHeight = 2048;
	public static readonly int kSubImageWidth = 16;
	public static readonly int kSubImageHeight = 16;

	public GameObject playerGO;
	public GameObject uiGO;
	public GameObject landscapeGO;
	public GameObject snapshotCameraGO;
	public GameObject commandPromptGO;

	public Material voxelMaterialExact;
	public Material voxelMaterialVolume;
	public Material voxelMaterialLit;
	public Texture2D textureAtlas;

	public Vector3 entityBaseScale = new Vector3(0.2f, 0.2f, 0.2f);

	[HideInInspector]
	public static Root instance;

	[HideInInspector]
	public NotificationManager notificationManager;
	[HideInInspector]
	public MeshManager meshManager;
	[HideInInspector]
	public UIManager uiManager;
	[HideInInspector]
	public LandscapeManager landscapeManager;
	[HideInInspector]
	public PlayerStartupScript player;
	[HideInInspector]
	public AtlasManager atlasManager;
	[HideInInspector]
	public EntityManager entityManager;
	[HideInInspector]
	public ProjectManager projectManager;
	[HideInInspector]
	public CommandPrompt commandPrompt;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;
	public const Lod kLodLit = 2;
	public const Lod kLodCount = kLodLit + 1;

	Root()
	{
		instance = this;
	}

	void Awake()
	{
		uiManager = uiGO.GetComponent<UIManager>();
		landscapeManager = landscapeGO.GetComponent<LandscapeManager>();
		player = playerGO.GetComponent<PlayerStartupScript>();
		commandPrompt = commandPromptGO.GetComponent<CommandPrompt>();

		meshManager = new MeshManager();
		entityManager = new EntityManager();
		projectManager = new ProjectManager();
		atlasManager = new AtlasManager();
		notificationManager = new NotificationManager();
	}

	void Start()
	{
		// Restore session on Start to ensure that all gameobjects
		// have been initialized on Awake, and started to subscribe
		// for notifications.
		projectManager.restoreSession();
	}

	public Material voxelMaterialForLod(Lod lod)
	{
		switch (lod) {
		case kLod0: return voxelMaterialExact;
		case kLod1: return voxelMaterialVolume;
		default: return voxelMaterialLit;
		}
	}

}
