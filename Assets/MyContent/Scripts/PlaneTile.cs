using UnityEngine;
using System.Collections;

public class PlaneTile : MonoBehaviour, ITile {

	public void initTile(GameObject gameObject, bool firstTile)
	{
	}

	public void moveTile(TileMoveDescription desc)
	{
		transform.position = desc.tileWorldPos;

		Mesh mesh = GetComponent<MeshFilter>().mesh;

		// todo: will this create a copy of the array? If so, can it be avoided?
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i)
			vertices[i].y = LandscapeConstructor.getGroundHeight(desc.tileWorldPos.x + vertices[i].x, desc.tileWorldPos.z + vertices[i].z);

		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		GetComponent<MeshCollider>().sharedMesh = null;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
}
