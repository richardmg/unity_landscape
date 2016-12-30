using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConstructionEditor : MonoBehaviour, IDragHandler, IPointerClickHandler
{
	public GameObject constructionCameraGO;
	public GameObject zoomSliderGo;
	public GameObject worldEntityButton;

	public float dragSpeedMin = 0.01f;
	public float dragSpeedMax = 0.2f;
	public float zoomMin = 10f;
	public float zoomMax = 200f;

	float m_dragPosX = 0;
	float m_dragPosY = 0;

	GameObject m_voxelObjectRootGo;
	GameObject m_selectedGameObject;

	bool m_moveEntity = false;

	void selectGameObject(GameObject go)
	{
		m_selectedGameObject = go;
		if (go) {
			m_dragPosX = m_selectedGameObject.transform.localPosition.x;
			m_dragPosY = m_selectedGameObject.transform.localPosition.y;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.dragging)
			return;

		RaycastHit hit;
		Camera camera = constructionCameraGO.GetComponent<Camera>();
		Ray ray = camera.ScreenPointToRay(new Vector3(eventData.pressPosition.x, eventData.pressPosition.y, Camera.main.nearClipPlane));
		LayerMask layerMask = ~LayerMask.NameToLayer("ConstructionCameraLayer");

		Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
		if (!hit.transform || hit.transform.gameObject == m_selectedGameObject)
			selectGameObject(null);
		else
			selectGameObject(hit.transform.gameObject);
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_selectedGameObject == null)
			return;
		
		float sliderValue = zoomSliderGo.GetComponent<Slider>().normalizedValue;
		float dragSpeed = dragSpeedMin + (sliderValue * (dragSpeedMax - dragSpeedMin));
		
		m_dragPosX += data.delta.x * dragSpeed;
		m_dragPosY += data.delta.y * dragSpeed;

		Vector3 pos = m_selectedGameObject.transform.localPosition;
		pos.x = Mathf.Floor(m_dragPosX);
		pos.y = Mathf.Floor(m_dragPosY);
		m_selectedGameObject.transform.localPosition = pos;
	}

	public void setEntityClass(EntityClass entityClass)
	{
		if (m_voxelObjectRootGo != null)
			m_voxelObjectRootGo.hideAndDestroy();

		VoxelObjectRoot root = entityClass.getVoxelObjectRoot();
		m_voxelObjectRootGo = root.createGameObject(transform, Root.kLod0);

		Vector3 cameraPos = root.snapshotOffset;
		constructionCameraGO.transform.localPosition = cameraPos;

		// Create VoxelObject GameObjects for each voxel object inside root
		m_voxelObjectRootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		Transform[] selfAndchildren = m_voxelObjectRootGo.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < selfAndchildren.Length; ++i) {
			GameObject go = selfAndchildren[i].gameObject;
			go.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		}

		// Set zoom slider at correct position
		float zoomNormalized = (cameraPos.magnitude - zoomMin) / (zoomMax - zoomMin);
		zoomSliderGo.GetComponent<Slider>().normalizedValue = zoomNormalized;
	}

	public VoxelObjectRoot createVoxelObjectRoot()
	{
		// Copy GameObject transforms back into VoxelObjectRoot
		VoxelObjectRoot root = new VoxelObjectRoot();
		root.snapshotOffset = constructionCameraGO.transform.localPosition;
		VoxelObjectMonoBehaviour[] vombs = m_voxelObjectRootGo.GetComponentsInChildren<VoxelObjectMonoBehaviour>(true);

		for (int i = 0; i < vombs.Length; ++i) {
			VoxelObjectMonoBehaviour vomb = vombs[i];
			VoxelObject vo = new VoxelObject(vomb.voxelObject.atlasIndex, vomb.voxelObject.voxelDepth);
			vo.localPosition = vomb.transform.localPosition;
			vo.localRotation = vomb.transform.localRotation;
			root.voxelObjects.Add(vo);
		}

		return root;
	}

	public void onZoomSliderChanged(Slider slider)
	{
		float zoom = -zoomMin + (slider.normalizedValue * -(zoomMax - zoomMin));
		Vector3 cameraPos = new Vector3(0, 0, zoom);
		constructionCameraGO.transform.localPosition = cameraPos;
		constructionCameraGO.transform.LookAt(m_voxelObjectRootGo.transform.position);
	}

	public void onWorldEntityButtonClicked()
	{
		m_moveEntity = !m_moveEntity;
		worldEntityButton.GetComponentInChildren<Text>().text = m_moveEntity ? "Entity" : "World";
	}

	public void onAddButtonClicked()
	{
		VoxelObject vo = new VoxelObject(0, 4);
		GameObject voxelObjectGo = vo.createGameObject(m_voxelObjectRootGo.transform, Root.kLod0, false);

		voxelObjectGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");

		selectGameObject(voxelObjectGo);
	}

	void addVoxelObject(VoxelObject vo)
	{
	}
}
