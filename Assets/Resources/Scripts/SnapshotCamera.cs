using UnityEngine;
using System.Collections;

public class SnapshotCamera {

	Camera m_camera;
	GameObject m_cameraGO;

	public SnapshotCamera(GameObject cameraGO)
	{
		m_cameraGO = cameraGO;
		m_camera = cameraGO.GetComponent<Camera>();
	}

	public Texture2D takeSnapshot(GameObject targetGO, Vector3 cameraOffset)
	{
		Texture2D snapshot = new Texture2D(m_camera.targetTexture.width, m_camera.targetTexture.height);
		Rect targetRect = new Rect(0, 0, snapshot.width, snapshot.height);
		takeSnapshot(targetGO, cameraOffset, snapshot, targetRect);
		snapshot.Apply();
		return snapshot;
	}

	public void takeSnapshot(GameObject targetGO, Vector3 cameraOffset, Texture2D destTexture, Rect destRect)
	{
        RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = m_camera.targetTexture;
		int prevLayer = targetGO.layer;

		m_cameraGO.transform.parent = targetGO.transform.parent;
		m_cameraGO.transform.localPosition = targetGO.transform.localPosition + cameraOffset;
		m_cameraGO.transform.LookAt(targetGO.transform.position);
		targetGO.layer = LayerMask.NameToLayer("SnapshotCameraLayer");

		m_camera.Render();
//		destTexture.Apply() - remember to do this in the end

		int srcX = (int)(m_camera.targetTexture.width - destRect.width) / 2;
		int srcY = (int)(m_camera.targetTexture.height - destRect.height) / 2;
		destTexture.ReadPixels(new Rect(srcX, srcY, destRect.width, destRect.height), (int)destRect.x, (int)destRect.y);

		targetGO.layer = prevLayer;
        RenderTexture.active = currentRT;
	}
}
