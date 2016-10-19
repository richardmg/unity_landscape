﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class AtlasManager : IProjectIOMember
{
	public Texture2D textureAtlas;
	int currentIndex = 0;

	public int acquireIndex()
	{
		return currentIndex++;
	}

	public void releaseIndex(int index)
	{
	}

	public void atlasPixelForIndex(int atlasIndex, out int x, out int y)
	{
		x = (atlasIndex * Root.kSubImageWidth) % Root.kAtlasWidth;
		y = (int)((atlasIndex * Root.kSubImageWidth) / Root.kAtlasHeight) * Root.kSubImageHeight;
	}

	public void copySubImage(int srcIndex, int destIndex)
	{
		if (srcIndex == destIndex)
			return;

//		Debug.Log("copy " + srcIndex + " to " + destIndex);

		int srcX, srcY, destX, destY;
		atlasPixelForIndex(srcIndex, out srcX, out srcY);
		atlasPixelForIndex(destIndex, out destX, out destY);
		Color[] pixels = textureAtlas.GetPixels(srcX, srcY, Root.kSubImageWidth, Root.kSubImageHeight);
		textureAtlas.SetPixels(destX, destY, Root.kSubImageWidth, Root.kSubImageHeight, pixels);
		textureAtlas.Apply();
	}

	public void syncMaterialsWithAtlas()
	{
		Debug.Assert(textureAtlas);
		Debug.Assert(textureAtlas.width == Root.kAtlasWidth);
		Debug.Assert(textureAtlas.height == Root.kAtlasHeight);

		Root.instance.voxelMaterialExact.mainTexture = textureAtlas;
		Root.instance.voxelMaterialVolume.mainTexture = textureAtlas;
		Root.instance.voxelMaterialLit.mainTexture = textureAtlas;
	}

	public void initNewProject()
	{
		Texture2D defaultAtlas = Root.instance.textureAtlas;
		textureAtlas = new Texture2D(defaultAtlas.width, defaultAtlas.height);
		textureAtlas.filterMode = FilterMode.Point;
		textureAtlas.SetPixels32(defaultAtlas.GetPixels32());
		textureAtlas.Apply();

		syncMaterialsWithAtlas();
	}

	public void load(ProjectIO projectIO)
	{
		int imageByteCount = projectIO.readInt();
		byte[] imageBytes = new byte[imageByteCount];
		projectIO.stream.Read(imageBytes, 0, imageBytes.Length);
		textureAtlas = new Texture2D(2, 2);
		textureAtlas.filterMode = FilterMode.Point;
		textureAtlas.LoadImage(imageBytes);

		syncMaterialsWithAtlas();
	}

	public void save(ProjectIO projectIO)
	{
		byte[] bytes = textureAtlas.EncodeToPNG();
		projectIO.writeInt(bytes.Length);
		projectIO.stream.Write(bytes, 0, bytes.Length);

		// File.WriteAllBytes(path + "/atlas.png", bytes);
	}
}
