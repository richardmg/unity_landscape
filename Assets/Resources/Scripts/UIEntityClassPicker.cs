using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour, EntityListener {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;
	public GameObject selectionRectGO;

	Texture2D tableTexture;
	Color32[] clearColorArray;
	List<EntityClass> entityClasses;
	bool m_dirty = true;

	int rowCount = 10;
	int colCount = 10;
	int cellWidth = 50;
	int cellHeight = 50;
	int textureCellWidth = 2048 / 10;
	int textureCellHeight = 2048 / 10;
	int margin = 5;

	int selectedIndex;

	void Start()
	{
		Root.instance.notificationManager.addEntityListener(this);
	}

	void OnEnable()
	{
		if (tableTexture == null) {
			Vector2 selectionRectSize = new Vector2(cellWidth + (margin * 2), cellHeight + (margin * 2));
			Vector2 textureTableSize = new Vector2(textureCellWidth * colCount, textureCellHeight * rowCount);

			selectionRectGO.GetComponent<RawImage>().rectTransform.sizeDelta = selectionRectSize;

			tableTexture = new Texture2D((int)textureTableSize.x, (int)textureTableSize.y);
			rawImageGO.GetComponent<RawImage>().texture = tableTexture;
			clearColorArray = tableTexture.GetPixels32();
			for (int i = 0; i < clearColorArray.Length; i++)
				clearColorArray[i] = Color.red;//Color.clear;
		}

		if (m_dirty)
			repaintTableTexture();

		selectIndex(selectedIndex);
	}


	void Update()
    {
		if (!Input.GetMouseButtonDown(0))
			return;

		Vector2 uv = UIManager.getMousePosOnImage(rawImageGO.GetComponent<RawImage>(), true);
		if (!UIManager.isInside(uv))
			return;

		int x = (int)(uv.x * colCount);
		int y = (int)(uv.y * rowCount);
		int index = x + (y * colCount);
		if (index < 0 || index >= entityClasses.Count)
			return;

		if (selectedIndex != index)
			selectIndex(index);
		else
			Root.instance.uiManager.showFirstPersonUI();

//		EntityClass entityClass = entityClasses[index];
//		Root.instance.player.currentEntityClass = entityClass;
//
//		if (Input.GetKey(KeyCode.LeftShift)) {
//			Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
//			Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
//		} else if (Input.GetKey(KeyCode.LeftControl)) {
//			EntityClass newEntityClass = new EntityClass(entityClass);
//			Root.instance.player.currentEntityClass = newEntityClass;
//			repaintTableTexture();
//		} else {
//			Root.instance.uiManager.showFirstPersonUI();
//		}
	}

	public void selectIndex(int index)
	{
		selectedIndex = index;
		moveSelectionRect(index);
		EntityClass entityClass = entityClasses[index];
		Root.instance.player.currentEntityClass = entityClass;
	}

	public void moveSelectionRect(int index)
	{
		float w = tableTexture.width;
		float h = tableTexture.height;
		float topX = rawImageGO.transform.position.x - (w / 2);
		float topY = rawImageGO.transform.position.y - (h / 2);

		int cellX, cellY;
		cellPos(index, out cellX, out cellY);
//		cellX -= margin;
//		cellY -= margin;
	
		cellX += textureCellWidth / 2;
		cellY += textureCellHeight / 2;
		selectionRectGO.transform.position = new Vector3(topX + cellX, topY + cellY, 0);
	}

	public void onEntityClassAdded(EntityClass entityClass)
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;
		
		paintEntityClass(entityClass);
		paintingDone();
	}

	public void onEntityClassChanged(EntityClass entityClass)
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;

		paintEntityClass(entityClass);
		paintingDone();
	}

	public void onEntityInstanceAdded(EntityInstance entityInstance)
	{
	}

	void repaintTableTexture()
	{
		tableTexture.SetPixels32(clearColorArray);
		entityClasses = Root.instance.entityManager.allEntityClasses;

		for (int id = 0; id < entityClasses.Count; ++id)
			paintEntityClass(Root.instance.entityManager.getEntity(id));

		paintingDone();
	}

	void paintingDone()
	{
		tableTexture.Apply();
		m_dirty = false;
	}

	void paintEntityClass(EntityClass entityClass)
	{
		// NB: I assume here that an entities ID correspond to the
		// cell in the tabletexture. This might change in the future...
		int id = entityClass.id;
		int x, y;
		cellPos(id, out x, out y);
		entityClasses[id].takeSnapshot(tableTexture, new Rect(x, y, textureCellWidth, textureCellHeight));
	}

	void cellPos(int index, out int x, out int y)
	{
		x = (index * textureCellWidth) % tableTexture.width;
		y = (int)((index * textureCellWidth) / tableTexture.width) * textureCellHeight;
		y = (int)tableTexture.height - textureCellHeight - y;
	}

	public void onCloneButtonClicked()
	{
		EntityClass entityClass = new EntityClass(Root.instance.entityManager.getEntity(selectedIndex));
		selectIndex(entityClass.id);
	}
}
