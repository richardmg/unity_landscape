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
		int prevLayer = targetGO.layer;

		m_cameraGO.transform.parent = targetGO.transform.parent;
		m_cameraGO.transform.localPosition = targetGO.transform.localPosition + cameraOffset;
		m_cameraGO.transform.LookAt(targetGO.transform.position);
		targetGO.layer = LayerMask.NameToLayer("SnapshotCameraLayer");

        RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = m_camera.targetTexture;
		m_camera.Render();

		Texture2D snapshot = new Texture2D(m_camera.targetTexture.width, m_camera.targetTexture.height);
		snapshot.ReadPixels(new Rect(0, 0, m_camera.targetTexture.width, m_camera.targetTexture.height), 0, 0);
		snapshot.Apply();

		targetGO.layer = prevLayer;
        RenderTexture.active = currentRT;
		return snapshot;
	}
}
