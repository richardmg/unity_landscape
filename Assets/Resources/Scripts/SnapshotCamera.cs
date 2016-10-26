using UnityEngine;
using System.Collections;

public class SnapshotCamera {

	public Vector3 targetOffset;

	Camera m_camera;
	GameObject m_cameraGO;
	RenderTexture renderTexture;

	public SnapshotCamera(int renderTextureWidth = 256, int renderTextureHeight = 256, float targetOffset = -10)
	{
		this.targetOffset = new Vector3(0, 0, targetOffset);
		renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.ARGB32);

		m_cameraGO = Root.instance.snapshotCameraGO;
		m_camera = m_cameraGO.GetComponent<Camera>();
	}

	public Texture2D takeSnapshot(GameObject targetGO)
	{
		Texture2D targetTexture = new Texture2D(renderTexture.width, renderTexture.height);
		takeSnapshot(targetGO, targetTexture, 0, 0);
		targetTexture.Apply();
		return targetTexture;
	}

	public void takeSnapshot(GameObject targetGO, Texture2D destTexture, int destX, int destY)
	{
		renderTexture.Create();
		m_camera.targetTexture = renderTexture;

        RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = renderTexture;
		int prevLayer = targetGO.layer;

		Bounds bounds = targetGO.GetComponent<Renderer>().bounds;
		m_cameraGO.transform.parent = targetGO.transform.parent;
		m_cameraGO.transform.localPosition = targetGO.transform.localPosition + targetOffset + bounds.center;
		m_cameraGO.transform.LookAt(bounds.center);
		targetGO.layer = LayerMask.NameToLayer("SnapshotCameraLayer");

		m_camera.Render();
//		destTexture.Apply() - remember to do this in the end

		destTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), destX, destY);

		targetGO.layer = prevLayer;
        RenderTexture.active = currentRT;
	}
}
