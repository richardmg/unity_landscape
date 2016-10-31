using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour, EntityListener, ProjectListener {

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

		// Calculate the size of the selection rectangle
		Vector2 imageSize = image.rectTransform.sizeDelta;
		float selectionRectWidth = textureCellWidth * (imageSize.x / textureWidth);
		float selectionRectHeight = textureCellHeight * (imageSize.y / textureHeight);
		Vector2 selectionRect = new Vector2(selectionRectWidth, selectionRectHeight);
		selectionRectGO.GetComponent<RawImage>().rectTransform.sizeDelta = selectionRect;

		moveSelectionRect(selectedIndex);

		Root.instance.notificationManager.addProjectListener(this);
		Root.instance.notificationManager.addEntityListener(this);
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
			Root.instance.uiManager.showFirstPersonUI();
	}

	public void selectEntityClass(EntityClass entityClass)
	{
		// This is propbably wrong once id doesn't map to index!
		selectIndex(entityClass.id);
	}

	public EntityClass getSelectedEntityClass()
	{
		// This is propbably wrong once id doesn't map to index!
		return Root.instance.entityManager.getEntity(selectedIndex);
	}

	public void selectIndex(int index)
	{
		selectedIndex = index;
		if (tableTexture != null)
			moveSelectionRect(index);
	}

	public void moveSelectionRect(int index)
	{
		int x, y;
		textureCellPos(index, out x, out y);
		x += textureCellWidth / 2;
		y += textureCellHeight / 2;
		textureToImagePos(ref x, ref y);
		selectionRectGO.transform.position = new Vector3(x, y, 0);
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

	public void onEntityInstanceAdded(EntityInstance entityInstance)
	{
	}

	public void onEntityInstanceSwapped(EntityInstance from, EntityInstance to)
	{
	}

	void repaintTableTexture()
	{
		tableTexture.SetPixels(clearColorArray);
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;

		for (int id = 0; id < entityClasses.Count; ++id)
			paintEntityClass(id, Root.instance.entityManager.getEntity(id));

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
		x = (index * textureCellWidth) % tableTexture.width;
		y = (int)((index * textureCellWidth) / tableTexture.width) * textureCellHeight;
		y = (int)tableTexture.height - textureCellHeight - y;
	}

	void textureToImagePos(ref int x, ref int y)
	{
		float w = image.rectTransform.sizeDelta.x;
		float h = image.rectTransform.sizeDelta.y;
		float topX = rawImageGO.transform.position.x - (w / 2);
		float topY = rawImageGO.transform.position.y - (h / 2);

		x = (int)topX + (int)(x * (w / tableTexture.width));
		y = (int)topY + (int)(y * (h / tableTexture.height));
	}

	public void onCloseButtonClicked()
	{
		Root.instance.uiManager.showFirstPersonUI();
	}

	public void onCloneButtonClicked()
	{
		EntityClass originalEntityClass = Root.instance.entityManager.getEntity(selectedIndex);
		if (originalEntityClass == null)
			return;
		EntityClass entityClass = new EntityClass(originalEntityClass);
		selectIndex(entityClass.id);
	}

	public void onDeleteButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityManager.getEntity(selectedIndex);
		if (entityClass == null)
			return;
		entityClass.remove();
	}

	public void onPaintButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityManager.getEntity(selectedIndex);
		if (entityClass == null)
			return;
		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.push(Root.instance.uiManager.uiPaintEditorGO, (bool accepted) => {});
	}

}
