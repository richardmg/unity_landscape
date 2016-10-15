using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	public GameObject uiEntityPickerCamera;

	EntityInstance instance;

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			instance = entityClasses[i].createInstance(uiEntityPickerCamera.transform);
			instance.gameObject.layer = LayerMask.NameToLayer("UIEntityPickerLayer");
			instance.makeStandalone();
			instance.transform.localPosition = new Vector3(0, 0, 1);
			float scale = 0.01f;
			instance.gameObject.transform.localScale = new Vector3(scale, scale, scale);
		}
	}

	void OnDisable()
	{
		GameObject.Destroy(instance.gameObject);
	}

	public void onNewTreeButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}
}
