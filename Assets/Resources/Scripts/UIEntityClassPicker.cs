using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;

	void OnEnable()
	{
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			Texture2D snapshot = entityClasses[i].takeSnapshot();
			rawImageGO.GetComponent<RawImage>().texture = snapshot;
		}
	}

	void OnDisable()
	{
	}

	public void onNewTreeButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}
}
