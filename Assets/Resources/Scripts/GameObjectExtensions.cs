using System;
using UnityEngine;
using System.Collections;
using Lod = System.Int32;
using VoxelRotation = UnityEngine.Vector3;

public static class UIManager_GameObjectExtensions
{
	public static void hideAndDestroy(this GameObject go)
	{
		go.SetActive(false);
		GameObject.Destroy(go);
//		UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
	}

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

	public static Mesh createCombinedMesh(this GameObject go, Lod lod)
	{
		MeshFilter[] selfAndchildren = go.GetComponentsInChildren<MeshFilter>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = go.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			MeshFilter filter = selfAndchildren[i];
			combine[i].mesh = filter.sharedMesh;
			combine[i].transform = parentTransform * filter.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}

	public static void setVoxelRotation(this Transform transform, VoxelRotation rotation)
	{
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.Rotate(0, rotation.y, 0, Space.Self);
		transform.Rotate(rotation.x, 0, 0, Space.Self);
		transform.Rotate(0, rotation.z, 0, Space.Self);
	}

}
