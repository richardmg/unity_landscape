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
		// Note: The difference from using Quaternion directly is that we choose
		// to apply y rotation first. This makes them easier to work with when
		// rotating them in the UI. Also, the rotation should set either x or z, not
		// both, as this becomes confusing to work with in the UI.
		Debug.Assert(rotation.x == 0 || rotation.z == 0);
		transform.rotation = Quaternion.Euler(0, 0, 0);
		transform.Rotate(0, rotation.y, 0, Space.Self);
		transform.Rotate(rotation.x, 0, 0, Space.Self);
		transform.Rotate(0, 0, rotation.z, Space.Self);
	}

	public static Vector3 getVoxelPushDirection(this Transform pusher, Transform pushed, Space space)
	{
		// Return the direction the first selected object is being pushed by the
		// user. The direction will only be one out of the pushed transforms local x or z
		Vector3 pusherForward = pusher.forward;
		Vector3 direction = Vector3.zero;
		float dist = Mathf.Infinity;
		pusher.selectNearest(ref direction, ref dist, pushed.forward, (space == Space.World ? pushed.forward : Vector3.forward), pusherForward);
		pusher.selectNearest(ref direction, ref dist, pushed.right, (space == Space.World ? pushed.right : Vector3.right), pusherForward);
		pusher.selectNearest(ref direction, ref dist, pushed.up, (space == Space.World ? pushed.up : Vector3.up), pusherForward);
		pusher.selectNearest(ref direction, ref dist, pushed.forward * -1, (space == Space.World ? pushed.forward : Vector3.forward) * -1, pusherForward);
		pusher.selectNearest(ref direction, ref dist, pushed.right * -1, (space == Space.World ? pushed.right : Vector3.right) * -1, pusherForward);
		pusher.selectNearest(ref direction, ref dist, pushed.up * -1, (space == Space.World ? pushed.up : Vector3.up) * -1, pusherForward);
		direction.y = 0;
		direction.Normalize();
		return direction;
	}

	private static void selectNearest(this Transform transform, ref Vector3 currentDirection,
		ref float currentDist, Vector3 otherDirection, Vector3 otherDirectionSubstitute, Vector3 forward)
	{
		float dist = Vector3.Distance(otherDirection, forward);
		if (dist < currentDist) {
			currentDirection = otherDirectionSubstitute;
			currentDist = dist;
		}
	}

}
