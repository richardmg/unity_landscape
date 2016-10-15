﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			EntityInstance instance = entityClasses[i].createInstance(Root.instance.playerGO.transform, "Entity in picker");
			instance.makeStandalone();
			instance.transform.localPosition = new Vector3(0, 0, 10);
			instance.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
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
