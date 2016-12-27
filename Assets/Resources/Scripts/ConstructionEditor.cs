using System;
using UnityEngine;
using System.Collections;
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
		
		m_voxelObjectRootGo = entityClass.getVoxelObjectRoot().createGameObject(transform, Root.kLod0);
		m_voxelObjectRootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
	}

	public VoxelObjectRoot takeVoxelObjectRoot()
	{
		// Copy GameObject transforms back into VoxelObjectRoot
		VoxelObjectRoot root = new VoxelObjectRoot();
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

		System.Random rnd = new System.Random();
		float x = rnd.Next(0, 30);
		float y = rnd.Next(0, 30);
		voxelObjectGo.transform.localPosition = new Vector3(x, y, 0);
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
