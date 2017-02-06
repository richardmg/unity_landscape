using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIEntityClassPicker : MonoBehaviour, IPointerDownHandler, IEntityClassListener, IProjectListener {

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
		// Note: geometry calculations cannot be done on Awake()
		OnRectTransformDimensionsChange();
	}

	void OnEnable()
	{
		if (m_dirty)
			repaintTableTexture();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Vector2 uv = UIManager.getMousePosInsideRect(rawImageGO.GetComponent<RectTransform>(), true);
		if (!UIManager.isInside(uv))
			return;

		int x = (int)(uv.x * colCount);
		int y = (int)(uv.y * rowCount);
		int index = x + (y * colCount);

		int prevIndex = selectedIndex;
		if (index != selectedIndex)
			selectIndex(index);

		if (Input.GetKey(KeyCode.LeftShift))
			onEditButtonClicked();
		else if (Input.GetKey(KeyCode.LeftControl))
			onCloneButtonClicked();
		else if (prevIndex == selectedIndex)
			Root.instance.uiManager.background.onOkButtonClicked();
	}

	void OnRectTransformDimensionsChange()
	{
		// Recalculate size of selection rect when canvas rect is resized
		if (!image)
			return;
		
		Rect r = image.rectTransform.rect;
		float w = r.width / rowCount;
		float h = r.height / colCount;
		w += selectionRectMargin * 2;
		h += selectionRectMargin * 2;
		selectionRectGO.GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(w, h);
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

	public void onEditButtonClicked()
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(selectedIndex);
		bool createTemporary = (entityClass == null);

		if (createTemporary) {
			// Create a temporary entity class with one voxel object. If the
			// user clicks "ok" after painting, it will be stored.
			// Note: selected index logic needs to change for the created
			// voxel object. Ask instead for an available slot in the
			// atlas. SelectedIndex points to available EntityClasses, and
			// are not be atlas indices.
			entityClass = new EntityClass("New entity", selectedIndex);
			entityClass.voxelObjectRoot.add(new VoxelObject(selectedIndex, 1f));
		}

		Root.instance.uiManager.entityPainter.setEntityClass(entityClass);
		Root.instance.uiManager.uiPaintEditorGO.pushDialog((bool accepted) => {
			if (accepted)
				Root.instance.notificationManager.notifyEntityClassChanged(entityClass);
			else if (createTemporary)
				entityClass.remove();
		});
	}

}
