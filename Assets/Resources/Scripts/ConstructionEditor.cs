using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ConstructionEditor : MonoBehaviour {

	public GameObject constructionCameraGO;
	public GameObject zoomSlider;
	public GameObject worldEntityButton;

	GameObject m_voxelObjectRootGo;

	bool m_moveEntity = false;

	// Use this for initialization
	void Start () {
		zoomSlider.GetComponent<Slider>().normalizedValue = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setEntityClass(EntityClass entityClass)
	{
		if (m_voxelObjectRootGo != null)
			m_voxelObjectRootGo.hideAndDestroy();

		VoxelObjectRoot root = entityClass.getVoxelObjectRoot();
		m_voxelObjectRootGo = root.createGameObject(transform, Root.kLod0);
		m_voxelObjectRootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");

		constructionCameraGO.transform.localPosition = root.snapshotOffset;

		for (int i = 0; i < root.voxelObjects.Count; ++i) {
			VoxelObject vo = root.voxelObjects[i];
			GameObject voxelObjectGo = vo.createGameObject(m_voxelObjectRootGo.transform, Root.kLod0);
			voxelObjectGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		}
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
		Vector3 cameraPos = new Vector3(0, 0, slider.normalizedValue * -200);
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
