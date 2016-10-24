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

	int rowCount = 5;
	int colCount = 5;
	int cellWidth = 250;
	int cellHeight = 250;

	void Start()
	{
		Root.instance.notificationManager.addEntityListener(this);
	}

	void OnEnable()
	{
		if (tableTexture == null) {
			tableTexture = new Texture2D(cellWidth * colCount, cellHeight * rowCount);
			rawImageGO.GetComponent<RawImage>().texture = tableTexture;
			clearColorArray = tableTexture.GetPixels32();
			for (int i = 0; i < clearColorArray.Length; i++)
				clearColorArray[i] = Color.clear;
		}

		if (m_dirty)
			repaintTableTexture();
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
		
		EntityClass entityClass = entityClasses[index];
		Root.instance.player.currentEntityClass = entityClass;

		if (Input.GetKey(KeyCode.LeftShift)) {
			Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
			Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
		} else if (Input.GetKey(KeyCode.LeftControl)) {
			EntityClass newEntityClass = new EntityClass(entityClass);
			Root.instance.player.currentEntityClass = newEntityClass;
			repaintTableTexture();
		} else {
			Root.instance.uiManager.showFirstPersonUI();
		}
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
		int x = (id * cellWidth) % tableTexture.width;
		int y = (int)((id * cellWidth) / tableTexture.width) * cellHeight;
		y = (int)tableTexture.height - cellHeight - y;
		entityClasses[id].takeSnapshot(tableTexture, new Rect(x, y, cellWidth, cellHeight));
	}

	public void onCloneButtonClicked()
	{
		EntityClass entityClass = new EntityClass("SquareTree");
		Root.instance.player.currentEntityClass = entityClass;
	}
}
