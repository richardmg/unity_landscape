using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConstructionEditor : MonoBehaviour {

	public GameObject constructionCameraGO;
	EntityInstance m_instance;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setEntityClass(EntityClass entityClass)
	{
		// - Ønsker å fjerne index substitutions
		// - Ønsker å fjerne prefab
		// - Ønsker å kunne bygge hierarki med VoxelObjectRoot-s
		// - EntityClass vil fortsatt bare ha en (master) root.
		// - Constructor vil bare jobbe på barn direkte under entity class root
		// - Ergo ønsker jeg her ikke deep copy, men bare copy av direkte barn
		// - VoxelObjectRoot vil dermed også ha en transform
		// - Tror jeg dropper at man kan bygge hierarki med VoxelObjects, kun
		// 		med VoxelObjectRoot.
		// - VoxelObject blir dermed leaf-noder.
		if (m_instance)
			m_instance.hideAndDestroy();

//		m_instance = entityClass.createInstance(transform, "ConstructionEntity");
//		m_instance.transform.localPosition = Vector3.zero;
//		m_instance.makeStandalone(Root.kLodLit, true);


		/*
		// Create root to hold the children in the scene. Note that we
		// only end up with direct children of the root, and no grandchildren.
		GameObject rootGo = new GameObject();
		rootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		rootGo.transform.parent = transform;
		rootGo.transform.localPosition = Vector3.zero;
		rootGo.AddComponent<VoxelObjectRoot>();

		// Foreach child voxelobject (leafs);
		// Copy voxel object and add to root
		GameObject voxelObjectGo = new GameObject();
		voxelObjectGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		voxelObjectGo.transform.parent = rootGo.transform;
		voxelObjectGo.transform.localPosition = Vector3.zero;
		VoxelObject vo = voxelObjectGo.AddComponent<VoxelObject>();
		vo.atlasIndex = 0;
		vo.makeStandalone(Root.kLod0);

		// Foreach child voxelobjectroot:
		Mesh childRootMesh = null; // shilcRoot.createMesh();
		GameObject childRootGo = new GameObject();
		childRootGo.addMeshComponents(Root.kLod0, childRootMesh);
		childRootGo.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
		childRootGo.transform.parent = rootGo.transform;
		childRootGo.transform.localPosition = Vector3.zero;
		*/
	}

	public void onZoomSliderChanged(Slider slider)
	{
		Vector3 cameraPos = new Vector3(0, 0, slider.normalizedValue * -200);
		constructionCameraGO.transform.localPosition = cameraPos;
	}

	public void onAddButtonClicked()
	{
		Root.instance.uiManager.uiEntityClassPickerGO.pushDialog((bool accepted) => {
			if (!accepted)
				return;
			addEntityClass(Root.instance.uiManager.entityClassPicker.getSelectedEntityClass());	
		});
	}

	void addEntityClass(EntityClass entityClass)
	{
		print("add: " + entityClass.ToString());
		EntityInstance instance = entityClass.createInstance(transform);
		instance.makeStandalone(Root.kLod0);
	}
}
