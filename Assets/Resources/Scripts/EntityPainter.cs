using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMode = System.Int32;

public class EntityPainter : MonoBehaviour {
	public GameObject thumbnailRowGO;

	Color color = Color.white;
	Texture2D m_texture;
	EntityClass m_entityClass;
	int m_currentListIndex;
	public int currentAtlasIndex;
	List<int> m_atlasIndexList;
	List<RawImage> m_thumbnailImageList = new List<RawImage>();
	EditMode m_currentMode = kPaintMode;
	bool m_textureDirty = false;
	bool m_clearToggleOn = false;

	const EditMode kPaintMode = 0;
	const EditMode kColorSelectMode = 1;

	void Awake()
	{
		createThumbnails();
	}

	void Start()
	{
		setEntityClass(Root.instance.player.currentEntityClass);
	}

	void OnDisable()
	{
		saveChanges();
	}

	void Update()
    {
		if (!Root.instance.uiManager.grabMouse(this))
			return;
		if (m_entityClass == null)
			return;

		Vector2 uv = UIManager.getMousePosOnImage(GetComponent<RawImage>());

		if (UIManager.isInside(uv)) {
			if (Input.GetKey(KeyCode.C) || m_currentMode == kColorSelectMode)
				updateColorSelect(uv);
			else if (m_currentMode == kPaintMode)
				updatePaint(uv);
		} else {
			if (!Input.GetMouseButtonDown(0))
				return;

			updateThumbnailSelect(uv);
		}
	}

	void updatePaint(Vector2 uv)
	{
		int pixelX = (int)(uv.x * m_texture.width);
		int pixelY = (int)(uv.y * m_texture.height);

		if (m_clearToggleOn || Input.GetKey(KeyCode.LeftShift))
			m_texture.SetPixel(pixelX, pixelY, Color.clear);
		else
			m_texture.SetPixel(pixelX, pixelY, color);

		m_texture.Apply();
		m_textureDirty = true;
	}

	void updateColorSelect(Vector2 uv)
	{
		int pixelX = (int)(uv.x * m_texture.width);
		int pixelY = (int)(uv.y * m_texture.height);
		color = m_texture.GetPixel(pixelX, pixelY);

		m_currentMode = kPaintMode;
	}

	void updateThumbnailSelect(Vector2 uv)
	{
		for (int i = 0; i < m_thumbnailImageList.Count; ++i) {
			RawImage image = m_thumbnailImageList[i];
			Vector2 thumbUv = UIManager.getMousePosOnImage(image);
			if (UIManager.isInside(thumbUv))
				setListIndex(i);
		}
	}

	public void setEntityClass(EntityClass entityClass)
	{
		m_entityClass = entityClass;
		m_atlasIndexList = m_entityClass.atlasIndexList();
		setListIndex(0);
		updateThumbnails();
	}

	public void setListIndex(int listIndex)
    {
		m_currentListIndex = listIndex;
		setAtlasIndex(m_atlasIndexList[listIndex]);
	}

	public void createThumbnails()
	{
		int count = 10;
		float thumbSize = 50;
		float margin = 10;

		for (int i = 0; i < count; ++i) {
			GameObject thumbnailGO = new GameObject("Thumbnail " + i);
			thumbnailGO.transform.SetParent(thumbnailRowGO.transform);

			RawImage image = thumbnailGO.AddComponent<RawImage>();
			image.texture = Root.instance.atlasManager.textureAtlas;

			float x = (thumbSize / 2) + (i * (thumbSize + margin));

			image.rectTransform.anchorMin = new Vector2(0, 0.5f);
			image.rectTransform.anchorMax = new Vector2(0, 0.5f);
			image.rectTransform.anchoredPosition = new Vector3(x, 0);
			image.rectTransform.sizeDelta = new Vector2(thumbSize, thumbSize);

			m_thumbnailImageList.Add(image);
		}
	}

	public void updateThumbnails()
	{
		int i = 0;
		for (; i < m_atlasIndexList.Count; ++i) {
			int atlasIndex = m_atlasIndexList[i];
			m_thumbnailImageList[i].uvRect = Root.instance.atlasManager.getUVRectForIndex(atlasIndex);
		}

		Rect emptyRect = new Rect();
		for (; i < m_thumbnailImageList.Count; ++i)
			m_thumbnailImageList[i].uvRect = emptyRect;
	}

	public void setAtlasIndex(int atlasIndex)
	{
		currentAtlasIndex = atlasIndex;
		int atlasPixelX, atlasPixelY;
		Root.instance.atlasManager.getAtlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		Texture2D texture = Root.instance.atlasManager.textureAtlas;
		var pixels = texture.GetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight);

		m_texture = new Texture2D(Root.kSubImageWidth, Root.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;

		m_textureDirty = false;
	}

	public void onDiscardButtonClicked()
	{
		setListIndex(m_currentListIndex);
	}

	public void saveChanges()
    {
		if (m_texture == null || !m_textureDirty)
			return;

		int atlasPixelX;
		int atlasPixelY;
		Root.instance.atlasManager.getAtlasPixelForIndex(currentAtlasIndex, out atlasPixelX, out atlasPixelY);
		Texture2D texture = Root.instance.atlasManager.textureAtlas;
		texture.SetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight, m_texture.GetPixels());
		texture.Apply();

		m_textureDirty = false;

		m_entityClass.markDirty(EntityClass.DirtyFlags.Mesh);
		Root.instance.notificationManager.notifyEntityClassChanged(m_entityClass);
    }

	public void onColorButtonClicked()
	{
		GameObject colorPicker = Root.instance.uiManager.uiColorPickerGO;
		Root.instance.uiManager.push(colorPicker, (bool accepted) => {
			if (accepted)
				color = colorPicker.GetComponentInChildren<ColorPicker>().selectedColor;
		});
	}

	public void onColorSamplerButtonClicked()
	{
		m_currentMode = kColorSelectMode;
		Root.instance.uiManager.clearMouseGrab();
	}

	public void onEraseToggleClicked(Toggle toggle)
	{
		m_clearToggleOn = toggle.isOn;
	}

	public void onClearButtonClicked()
	{
		for (int x = 0; x < m_texture.width; ++x) {
			for (int y = 0; y < m_texture.height; ++y) {
				m_texture.SetPixel(x, y, Color.clear);
			}
		}

		m_texture.Apply();
		m_textureDirty = true;
	}

}
