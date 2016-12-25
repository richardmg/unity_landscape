using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour, IEntityClassListener, IProjectListener {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;
	public GameObject selectionRectGO;

	RawImage image;
	Texture2D tableTexture;
	Color[] clearColorArray;
	SnapshotCamera snapshotCamera;
	int selectedIndex;
	bool m_dirty = true;

	const int rowCount = 5;
	const int colCount = 5;
	const int textureCellWidth = 64;
	const int textureCellHeight = 64;

	const int selectionRectMargin = 5;

	void Awake()
	{
		int textureWidth = textureCellWidth * colCount;
		int textureHeight = textureCellHeight * rowCount;

		snapshotCamera = new SnapshotCamera(textureCellWidth, textureCellHeight);

		// Create table texture, and assign it to the image ui component
		tableTexture = new Texture2D(textureWidth, textureHeight);
		image = rawImageGO.GetComponent<RawImage>();
		image.texture = tableTexture;

		// Create a color array to clear the table texture
		clearColorArray = tableTexture.GetPixels();
		for (int i = 0; i < clearColorArray.Length; i++)
			clearColorArray[i] = Color.clear;

		selectIndex(0);

		Root.instance.notificationManager.addProjectListener(this);
		Root.instance.notificationManager.addEntityClassListener(this);
	}

	void Start()
	{
		// Calculate size of selection rect. This has to be done on Start
		// to ensure that the geometry of the image has been set and scaled correctly
		Rect r = image.rectTransform.rect;
		float w = r.width / rowCount;
		float h = r.height / colCount;
		w += selectionRectMargin * 2;
		h += selectionRectMargin * 2;
		selectionRectGO.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(w, h);
	}

	void OnEnable()
	{
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

		int prevIndex = selectedIndex;
		if (index != selectedIndex)
			selectIndex(index);

		if (Input.GetKey(KeyCode.LeftShift))
			onPaintButtonClicked();
		else if (Input.GetKey(KeyCode.LeftControl))
			onCloneButtonClicked();
		else if (prevIndex == selectedIndex)
			Root.instance.uiManager.setMenuVisible(false);
	}

	public void selectEntityClass(EntityClass entityClass)
	{
		// This is propbably wrong once id doesn't map to index!
		selectIndex(entityClass.id);
	}

	public EntityClass getSelectedEntityClass()
	{
		// This is propbably wrong once id doesn't map to index!
		return Root.instance.entityClassManager.getEntity(selectedIndex);
	}

	public void selectIndex(int index)
	{
		selectedIndex = index;
		if (tableTexture != null) {
			int x, y;
			anchoredCellPos(index, out x, out y);
			x -= selectionRectMargin;
			y += selectionRectMargin;
			selectionRectGO.GetComponent<RawImage>().rectTransform.anchoredPosition = new Vector3(x, y, 0);
		}
	}

	public void onProjectLoaded()
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;
		repaintTableTexture();
	}

	public void onEntityClassAdded(EntityClass entityClass)
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;
		
		paintEntityClass(entityClass.id, entityClass);
		paintingDone();
	}

	public void onEntityClassRemoved(EntityClass entityClass)
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;
		
		clearCell(entityClass.id);
		paintingDone();
	}

	public void onEntityClassChanged(EntityClass entityClass)
	{
		m_dirty = true;
		if (!gameObject.activeSelf)
			return;

		paintEntityClass(entityClass.id, entityClass);
		paintingDone();
	}

	void repaintTableTexture()
	{
		tableTexture.SetPixels(clearColorArray);
		List<EntityClass> entityClasses = Root.instance.entityClassManager.allEntityClasses;

		for (int id = 0; id < entityClasses.Count; ++id)
			paintEntityClass(id, Root.instance.entityClassManager.getEntity(id));

		paintingDone();
	}

	void paintingDone()
	{
		tableTexture.Apply();
		m_dirty = false;
	}

	void paintEntityClass(int index, EntityClass entityClass)
	{
		int x, y;
		textureCellPos(index, out x, out y);
		entityClass.takeSnapshot(snapshotCamera, tableTexture, x, y);
	}

	void clearCell(int index)
	{
		int x, y;
		textureCellPos(index, out x, out y);
		tableTexture.SetPixels(x, y, textureCellWidth, textureCellHeight, clearColorArray);
	}

	void textureCellPos(int index, out int x, out int y)
	{
		x = index % rowCount;
		y = index / rowCount;
		x *= textureCellWidth;
		y *= textureCellHeight;
		y = (int)tableTexture.height - textureCellHeight - y;
	}

	void anchoredCellPos(int index, out int x, out int y)
	{
		anchoredCellPos(index % rowCount, index / rowCount, out x, out y);
	}

	void anchoredCellPos(int tableX, int tableY, out int x, out int y)
	{
		x = (int)(tableX * (image.rectTransform.rect.width / rowCount));
		y = (int)(-tableY * (image.rectTransform.rect.height / colCount));
	}

	public void onCloseButtonClicked()
	{
		Root.instance.uiManager.setMenuVisible(false);
	}

	public void onCloneButtonClicked()
	{
		EntityClass originalEntityClass = Root.instance.entityClassManager.getEntity(selectedIndex);
		if (originalEntityClass == null)
			return;
		EntityClass entityClass = new EntityClass(originalEntityClass);
		selectIndex(entityClass.id);
	}

	public void onDeleteButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(selectedIndex);
		if (entityClass == null)
			return;
		entityClass.remove();
	}

	public void onPaintButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(selectedIndex);
		if (entityClass == null)
			return;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.uiPaintEditorGO.pushDialog();
	}

	public void onEditButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(selectedIndex);
		if (entityClass == null)
			return;
		Root.instance.uiManager.constructionEditor.setEntityClass(entityClass);
		Root.instance.uiManager.uiConstructionEditorGO.pushDialog();
	}

}
