﻿using UnityEngine;
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

	public Material voxelMaterialExact;
	public Material voxelMaterialVolume;
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

	public const Lod kNoLod = -1;
	public const Lod kLod0 = 0;
	public const Lod kLod1 = 1;
	public const Lod kLodCount = 2;

	Root()
	{
		instance = this;
	}

	void Awake()
	{
		meshManager = new MeshManager();
		entityManager = new EntityManager();
		uiManager = uiGO.GetComponent<UIManager>();
		notificationManager = new NotificationManager();
		atlasManager = new AtlasManager();
		landscapeManager = landscapeGO.GetComponent<LandscapeManager>();
		player = playerGO.GetComponent<PlayerStartupScript>();

		Debug.Assert(voxelMaterialExact.mainTexture.width == kAtlasWidth);
		Debug.Assert(voxelMaterialExact.mainTexture.height == kAtlasHeight);
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
