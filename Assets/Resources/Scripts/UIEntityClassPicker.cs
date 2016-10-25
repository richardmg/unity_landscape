using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIEntityClassPicker : MonoBehaviour, EntityListener {

	public GameObject uiEntityPickerCameraGO;
	public GameObject rawImageGO;
	public GameObject selectionRectGO;

	RawImage image;
	Texture2D tableTexture;
	Color32[] clearColorArray;
	bool m_dirty = true;

	const int rowCount = 10;
	const int colCount = 10;
	const int textureCellWidth = 2048 / 10;
	const int textureCellHeight = 2048 / 10;
	const int margin = 5;

	int selectedIndex;

	void Awake()
	{
		// Create table texture, and assign it to the image ui component
		image = rawImageGO.GetComponent<RawImage>();
		Vector2 textureTableSize = new Vector2(textureCellWidth * colCount, textureCellHeight * rowCount);
		tableTexture = new Texture2D((int)textureTableSize.x, (int)textureTableSize.y);
		image.texture = tableTexture;

		// Create a color array to clear the table texture
		clearColorArray = tableTexture.GetPixels32();
		for (int i = 0; i < clearColorArray.Length; i++)
			clearColorArray[i] = Color.clear;

		// Calculate the size of the selection rectangle
		Vector2 imageSize = image.rectTransform.sizeDelta;
		float selectionRectWidth = textureCellWidth * (imageSize.x / textureTableSize.x);
		float selectionRectHeight = textureCellHeight * (imageSize.x / textureTableSize.x);
		Vector2 selectionRect = new Vector2(selectionRectWidth, selectionRectHeight);
		selectionRectGO.GetComponent<RawImage>().rectTransform.sizeDelta = selectionRect;

		selectIndex(0);
	}

	void Start()
	{
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

	public void selectIndex(int index)
	{
		selectedIndex = index;
		moveSelectionRect(index);
		Root.instance.player.currentEntityClass = Root.instance.entityManager.getEntity(index);
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
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;

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
		textureCellPos(id, out x, out y);
		List<EntityClass> entityClasses = Root.instance.entityManager.allEntityClasses;
		entityClasses[id].takeSnapshot(tableTexture, new Rect(x, y, textureCellWidth, textureCellHeight));
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

	public void onCloneButtonClicked()
	{
		EntityClass originalEntityClass = Root.instance.entityManager.getEntity(selectedIndex);
		if (originalEntityClass == null)
			return;
		EntityClass entityClass = new EntityClass(originalEntityClass);
		selectIndex(entityClass.id);
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
