using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class AtlasManager {

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

	public void load(FileStream filestream)
	{
		// Read image byte length
		byte[] intBytes = new byte[sizeof(int)];
		filestream.Read(intBytes, 0, intBytes.Length);
		int imageByteCount = BitConverter.ToInt32(intBytes, 0);

		// Read image
		byte[] imageBytes = new byte[imageByteCount];
		filestream.Read(imageBytes, 0, imageBytes.Length);
		textureAtlas = new Texture2D(2, 2);
		textureAtlas.LoadImage(imageBytes);

		textureAtlas.filterMode = FilterMode.Point;

		Debug.Assert(textureAtlas);
		Debug.Assert(textureAtlas.width == Root.kAtlasWidth);
		Debug.Assert(textureAtlas.height == Root.kAtlasHeight);

		Root.instance.voxelMaterialExact.mainTexture = textureAtlas;
		Root.instance.voxelMaterialVolume.mainTexture = textureAtlas;
	}

	public void save(FileStream filestream)
	{
		byte[] bytes = textureAtlas.EncodeToPNG();
		byte[] imageByteCount = BitConverter.GetBytes(bytes.Length);

		filestream.Write(imageByteCount, 0, imageByteCount.Length);
		filestream.Write(bytes, 0, bytes.Length);

		// File.WriteAllBytes(path + "/atlas.png", bytes);
	}
}
