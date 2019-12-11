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
	public GameObject playerHeadGO;
	public GameObject uiGO;
	public GameObject entityToolManagerGO;
	public GameObject landscapeGO;
	public GameObject snapshotCameraGO;
	public GameObject skyCameraGO;
	public GameObject commandPromptGO;
	public GameObject entityInstanceManagerGO;
	public GameObject worldScaleManagerGO;

	public Material voxelMaterialExact;
	public Material voxelMaterialVolume;
	public Material voxelMaterialLit;
	public Texture2D textureAtlas;

	[HideInInspector] public static Root instance;

	[HideInInspector] public NotificationManager notificationManager;
	[HideInInspector] public MeshManager meshManager;
	[HideInInspector] public UIManager uiManager;
	[HideInInspector] public EntityToolManager entityToolManager;
	[HideInInspector] public LandscapeManager landscapeManager;
	[HideInInspector] public PlayerStartupScript player;
	[HideInInspector] public AtlasManager atlasManager;
	[HideInInspector] public EntityClassManager entityClassManager;
	[HideInInspector] public EntityInstanceManager entityInstanceManager;
	[HideInInspector] public ProjectManager projectManager;
	[HideInInspector] public CommandPrompt commandPrompt;
	[HideInInspector] public AlignmentManager alignmentManager;
	[HideInInspector] public SkyCamera skyCamera;

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;
	public const Lod kLodLit = 2;
	public const Lod kLodCount = kLodLit + 1;

	Root()
	{
		instance = this;

		// Create notification manager before Awake to let all
		// subscribers subscribe from their own Awake.
		notificationManager = new NotificationManager();
	}

	void Awake()
	{
		// Manager components in the following list are those that also exists in the
		// editor tree. Managers that do that typically do so because they have properties
		// that can be tweaked directly in the editor.

		// The UIManager stores all 2D dialogs that can be presented to the user
		uiManager = uiGO.GetComponent<UIManager>();

		// The EntitiyToolManager stores all tools that the user can use
		// to create, paint move etc the objects in the scene. 
		entityToolManager = entityToolManagerGO.GetComponent<EntityToolManager>();

		// The LandscapeManager stores information about the the ground, like
		// the height and biom at a specific world pos.
		landscapeManager = landscapeGO.GetComponent<LandscapeManager>();

		// The EntityInstanceManager stores all entities in the world
		entityInstanceManager = entityInstanceManagerGO.GetComponent<EntityInstanceManager>();

		// PlayerStartupScript controls information about to user, like
		// which tool he's using, and how fast he can walk.
		player = playerGO.GetComponent<PlayerStartupScript>();

		// The CommandPrompt is the in-game debug/introspection interface
		commandPrompt = commandPromptGO.GetComponent<CommandPrompt>();

		// The AlignmentManager helps aligning entities in the world so
		// they are more easy to move around and rotate by the user
		alignmentManager = worldScaleManagerGO.GetComponent<AlignmentManager>();

		// The sky camera is a debug helper, showing the world from bird perspective
		skyCamera = skyCameraGO.GetComponent<SkyCamera>();

		// -------------------------------------------------------------------

		// The following components don't need an editor API, so we create them explicit from code

		// MeshMananger do nothing ATM
		meshManager = new MeshManager();

		// The EntityClassManager stores all entity classes that has been created.
		// Any EntityInstance in the world is an instance of an EntityClass.
		entityClassManager = new EntityClassManager();

		// The ProjectManager handles loading and saving a project
		projectManager = new ProjectManager();

		// The AtlasManager handles the texture atlas that contains all the
		// paintings done by the user, which will also be used as the shape
		// and texture of the EntityClasses. 
		atlasManager = new AtlasManager();
	}

	void Start()
	{
        // Restore session on Start to ensure that all gameobjects
        // have been initialized on Awake, and started to subscribe
        // for notifications.
        projectManager.restoreSession();
        //projectManager.createProject("defaultworld", true);
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
