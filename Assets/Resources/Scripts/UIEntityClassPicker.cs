using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;
	Texture2D tableTexture;
	Color32[] clearColorArray;

	int rowCount = 5;
	int colCount = 5;
	int cellWidth = 256;
	int cellHeight = 256;

	void OnEnable()
	{
		if (tableTexture == null) {
			tableTexture = new Texture2D(cellWidth * colCount, cellHeight * rowCount);
			rawImageGO.GetComponent<RawImage>().texture = tableTexture;
			clearColorArray = tableTexture.GetPixels32();
			for (int i = 0; i < clearColorArray.Length; i++)
				clearColorArray[i] = Color.clear;
		}

		tableTexture.SetPixels32(clearColorArray);

		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		for (int i = 0; i < entityClasses.Count; ++i) {
			int x = (i * cellWidth) % tableTexture.width;
			int y = (int)((i * cellWidth) / tableTexture.width) * cellHeight;
			y = (int)tableTexture.height - cellHeight - y;
			entityClasses[i].takeSnapshot(tableTexture, new Rect(x, y, cellWidth, cellHeight));
		}

		tableTexture.Apply();
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
