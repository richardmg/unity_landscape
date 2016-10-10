using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class Root : MonoBehaviour {
	public static readonly int kAtlasWidth = 64;
	public static readonly int kAtlasHeight = 64;
	public static readonly int kSubImageWidth = 16;
	public static readonly int kSubImageHeight = 8;

	public GameObject playerGO;
	public GameObject uiGO;
	public GameObject landscapeGO;

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

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;

	Root()
	{
		instance = this;
	}

	void Awake()
	{
		meshManager = new MeshManager();
		uiManager = uiGO.GetComponent<UIManager>();
		notificationManager = new NotificationManager();
		atlasManager = new AtlasManager();
		landscapeManager = landscapeGO.GetComponent<LandscapeManager>();
		player = playerGO.GetComponent<PlayerStartupScript>();
	}

	public static void atlasPixelForIndex(int atlasIndex, out int x, out int y)
	{
		x = (atlasIndex * Root.kSubImageWidth) % Root.kAtlasWidth;
		y = (int)((atlasIndex * Root.kSubImageWidth) / Root.kAtlasHeight) * Root.kSubImageHeight;
	}

	public static GameObject getPrefab(string prefabName)
    {
		return (GameObject)Resources.Load("Prefabs/" + prefabName, typeof(GameObject));
    }
}
