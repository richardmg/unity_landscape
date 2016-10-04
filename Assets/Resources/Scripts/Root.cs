using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Root : MonoBehaviour {
	public static readonly int kAtlasWidth = 64;
	public static readonly int kAtlasHeight = 64;
	public static readonly int kSubImageWidth = 16;
	public static readonly int kSubImageHeight = 8;

	public GameObject player;
	public GameObject ui;
	public GameObject landscape;

	[HideInInspector]
	public static Root instance;

	[HideInInspector]
	public NotificationCenter notificationCenter;
	[HideInInspector]
	public VoxelObjectCache meshCache;
	[HideInInspector]
	public UIManager uiManager;
	[HideInInspector]
	public LandscapeManager landscapeManager;

	Root()
	{
		instance = this;
	}

	void Awake()
	{
		notificationCenter = new NotificationCenter();
		meshCache = new VoxelObjectCache();
		uiManager = ui.GetComponent<UIManager>();
		landscapeManager = landscape.GetComponent<LandscapeManager>();
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
