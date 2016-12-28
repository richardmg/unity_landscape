using UnityEngine;
using System.Collections;

public class SnapshotCamera {

	Camera m_camera;
	GameObject m_cameraGO;
	RenderTexture renderTexture;

	public SnapshotCamera(int renderTextureWidth = 256, int renderTextureHeight = 256)
	{
		renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.ARGB32);
		renderTexture.name = "SnapshotTexture";
		m_cameraGO = Root.instance.snapshotCameraGO;
		m_camera = m_cameraGO.GetComponent<Camera>();
	}

	public Texture2D takeSnapshot(GameObject targetGO, Vector3 cameraOffset)
	{
		Texture2D targetTexture = new Texture2D(renderTexture.width, renderTexture.height);
		takeSnapshot(targetGO, cameraOffset, targetTexture, 0, 0);
		targetTexture.Apply();
		return targetTexture;
	}

	public void takeSnapshot(GameObject targetGO, Vector3 cameraOffset, Texture2D destTexture, int destX, int destY)
	{
		renderTexture.Create();
		m_camera.targetTexture = renderTexture;

		// Todo; need to calculate center in a different way
		Bounds bounds = new Bounds();//targetGO.GetComponent<Renderer>().bounds;

		m_cameraGO.transform.parent = targetGO.transform.parent;
		m_cameraGO.transform.localPosition = targetGO.transform.localPosition + cameraOffset + bounds.center;
		m_cameraGO.transform.LookAt(targetGO.transform.localPosition + bounds.center);

		int prevLayer = targetGO.layer;
		MeshFilter[] filters = targetGO.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter filter in filters) {
			Debug.Assert(filter.gameObject.layer == prevLayer);
			filter.gameObject.layer = LayerMask.NameToLayer("SnapshotCameraLayer");
		}

		m_camera.Render();

		foreach (MeshFilter filter in filters)
			filter.gameObject.layer = prevLayer;

		// Make the render texture the active render
		// target, and read pixels from it into destTexture.
		RenderTexture prevActive = RenderTexture.active;
		RenderTexture.active = renderTexture;
		destTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), destX, destY);
        RenderTexture.active = prevActive;

//		destTexture.Apply() - remember to do this in the end
	}

}
