﻿using System;
using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public static class UIManager_GameObjectExtensions
{
	public static void pushDialog(this GameObject go, bool show = true, bool repush = false)
	{
		Root.instance.uiManager.push(go, show, repush);
	}

	public static void pushDialog(this GameObject go, Action<bool> callback, bool show = true, bool repush = false)
	{
		Root.instance.uiManager.push(go, callback, show, repush);
	}

	public static void addMeshComponents(this GameObject go, Lod lod = Root.kLod0, Mesh mesh = null)
	{
		MeshFilter meshFilter = go.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = go.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = mesh;
		MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
		if (!meshRenderer)
			meshRenderer = go.AddComponent<MeshRenderer>();
		MeshCollider meshCollider = go.GetComponent<MeshCollider>();
		if (!meshCollider)
			meshCollider = go.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = meshFilter.sharedMesh;
		meshRenderer.sharedMaterial = Root.instance.voxelMaterialForLod(lod);
	}

}