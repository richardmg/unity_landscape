using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			EntityInstance instance = entityClasses[i].createInstance(null, "Entity in picker");
			instance.makeStandalone();
		}
	}

	public void onNewTreeButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}
}
