using UnityEngine;
using System.Collections;

public class PlaneTile : MonoBehaviour, ITile {

	public void moveTile(Vector2 tileGridCoord, Vector3 tileWorldPos)
	{
		transform.position = tileWorldPos;

		Mesh mesh = GetComponent<MeshFilter>().mesh;

		// todo: will this create a copy of the array? If so, can it be avoided?
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i)
			vertices[i].y = LandscapeConstructor.getGroundHeight(tileWorldPos.x + vertices[i].x, tileWorldPos.z + vertices[i].z);

		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		GetComponent<MeshCollider>().sharedMesh = null;
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
}
