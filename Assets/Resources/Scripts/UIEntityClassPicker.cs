using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	public GameObject uiEntityPickerCamera;

	List<EntityInstance> entityInstanceList = new List<EntityInstance>();

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i)
			createEntityInstance(entityClasses[i]);
	}

	void OnDisable()
	{
		foreach (EntityInstance i in entityInstanceList)
			GameObject.Destroy(i.gameObject);
		entityInstanceList = new List<EntityInstance>();
	}

	void createEntityInstance(EntityClass entityClass)
	{
		EntityInstance instance = entityClass.createInstance(uiEntityPickerCamera.transform, entityClass.name);
		instance.gameObject.layer = LayerMask.NameToLayer("UIEntityPickerLayer");
		instance.makeStandalone();
		instance.transform.localPosition = new Vector3(0, 0, 1);
		float scale = 0.01f;
		instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
		entityInstanceList.Add(instance);
	}

	public void onNewTreeButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}
}
