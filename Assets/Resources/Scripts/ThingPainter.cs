﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	VoxelObject m_topLevelVoxelObject;
	int m_currentListIndex;
	List<VoxelObject> m_voxelObjectsWithAtlasIndexList;

	void OnEnable ()
    {
		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Global.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Global.kSubImageHeight);
	}

	void Update ()
    {
		if (m_topLevelVoxelObject == null)
			return;
		if (!Input.GetMouseButton(0))
			return;

		Vector3[] corners = new Vector3[4];
		RawImage image = GetComponent<RawImage>();
		image.rectTransform.GetWorldCorners(corners);

		float uvx = (Input.mousePosition.x - corners[0].x) / (corners[2].x - corners[0].x);
		float uvy = (Input.mousePosition.y - corners[0].y) / (corners[2].y - corners[0].y);
		if (uvx < 0 || uvx > 1 || uvy < 0 || uvy > 1)
			return;

        int pixelX = (int)(uvx * Global.kSubImageWidth);
        int pixelY = (int)(uvy * Global.kSubImageHeight);

		m_texture.SetPixel(pixelX, pixelY, color);
		m_texture.Apply();
	}

	void setCurrentListIndex(int listIndex)
    {
		m_currentListIndex = listIndex;
		int atlasIndex = m_voxelObjectsWithAtlasIndexList[listIndex].atlasIndex();

		int atlasPixelX, atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}

	public void onIndexFieldEndInput(InputField indexField)
    {
		GameObject prefab = Global.getPrefab(indexField.text);
		if (prefab == null) {
			print("Could not find prefab!");
			return;
		}

		m_voxelObjectsWithAtlasIndexList = new List<VoxelObject>();
		VoxelObject[] voxelObjects = prefab.GetComponentsInChildren<VoxelObject>(true);

		for (int i = 0; i < voxelObjects.Length; ++i) {
			int atlasIndex = voxelObjects[i].atlasIndex();
			if (atlasIndex >= 0) {
				// Only add unique faces
				bool unique = true;
				for (int v = 0; v < m_voxelObjectsWithAtlasIndexList.Count; ++v) {
					if (m_voxelObjectsWithAtlasIndexList[v].atlasIndex() == atlasIndex) {
						unique = false;
						break;
					}
				}
				if (unique)
					m_voxelObjectsWithAtlasIndexList.Add(voxelObjects[i]);
			}
		}

		if (m_voxelObjectsWithAtlasIndexList.Count == 0) {
			print("Could not find any non-toplevel voxel objects in prefab!");
			return;
		}

		m_topLevelVoxelObject = voxelObjects[0];
		setCurrentListIndex(0);
    }

	public void onPrevButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex - 1;
		if (index < 0)
			index = m_voxelObjectsWithAtlasIndexList.Count - 1;
		setCurrentListIndex(index);
	}

	public void onNextButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex + 1;
		if (index >= m_voxelObjectsWithAtlasIndexList.Count)
			index = 0;
		setCurrentListIndex(index);
	}

	public void onSaveButtonClicked()
    {
		if (m_topLevelVoxelObject == null)
			return;

		int atlasIndex = m_voxelObjectsWithAtlasIndexList[m_currentListIndex].atlasIndex();

		int atlasPixelX;
		int atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();
    }
}
