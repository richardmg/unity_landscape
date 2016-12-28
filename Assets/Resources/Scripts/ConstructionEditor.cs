using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConstructionEditor : MonoBehaviour, IDragHandler	
{
	public GameObject constructionCameraGO;
	public GameObject zoomSliderGo;
	public GameObject worldEntityButton;

	public float dragSpeed = 0.3f;
	public float zoomMin = 0f;
	public float zoomMax = 200f;

	GameObject m_voxelObjectRootGo;
	GameObject m_selectedGameObject;

	bool m_moveEntity = false;

	// Use this for initialization
	void Start () {
		zoomSliderGo.GetComponent<Slider>().normalizedValue = 0.5f;
	}
	
	public void OnDrag(PointerEventData data)
	{
		Vector3 pos = m_selectedGameObject.transform.localPosition;
		pos.x += data.delta.x * dragSpeed;
		pos.y += data.delta.y * dragSpeed;
		m_selectedGameObject.transform.localPosition = pos;
	}

	public void setEntityClass(EntityClass entityClass)
	{
		if (m_voxelObjectRootGo != null)
			m_voxelObjectRootGo.hideAndDestroy();

		VoxelObjectRoot root = entityClass.getVoxelObjectRoot();
		m_voxelObjectRootGo = root.createGameObject(transform, Root.kLod0);
		m_voxelObjectRootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");

		Vector3 cameraPos = root.snapshotOffset;
		constructionCameraGO.transform.localPosition = cameraPos;

		// Create VoxelObject GameObjects for each voxel object inside root
		for (int i = 0; i < root.voxelObjects.Count; ++i) {
			VoxelObject vo = root.voxelObjects[i];
			GameObject voxelObjectGo = vo.createGameObject(m_voxelObjectRootGo.transform, Root.kLod0);
			voxelObjectGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
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
		float zoom = zoomMin + (slider.normalizedValue * -zoomMax);
		Vector3 cameraPos = new Vector3(0, 0, zoom);
		constructionCameraGO.transform.localPosition = cameraPos;
	}

	public void onWorldEntityButtonClicked()
	{
		m_moveEntity = !m_moveEntity;
		worldEntityButton.GetComponentInChildren<Text>().text = m_moveEntity ? "Entity" : "World";
	}

	public void onAddButtonClicked()
	{
		VoxelObject vo = new VoxelObject(0, 4);
		GameObject voxelObjectGo = vo.createGameObject(m_voxelObjectRootGo.transform, Root.kLod0);
		voxelObjectGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");

		m_selectedGameObject = voxelObjectGo;

//		System.Random rnd = new System.Random();
//		float x = rnd.Next(0, 200) - 100;
//		float y = rnd.Next(0, 200) - 100;
//		voxelObjectGo.transform.localPosition = new Vector3(x, y, 0);
	}

	public void onAddEntityButtonClicked()
	{
//		Root.instance.uiManager.uiEntityClassPickerGO.pushDialog((bool accepted) => {
//			if (!accepted)
//				return;
//			EntityClass entityClass = Root.instance.uiManager.entityClassPicker.getSelectedEntityClass();
//			if (entityClass != null)
//				addVoxelObject(entityClass);	
//		});
	}

	void addVoxelObject(VoxelObject vo)
	{
	}
}
