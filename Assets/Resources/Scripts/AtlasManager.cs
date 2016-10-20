using UnityEngine;
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

	public void copySubImageFromBaseToBase(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, Root.instance.textureAtlas, Root.instance.textureAtlas);
	}

	public void copySubImageFromBaseToProject(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, Root.instance.textureAtlas, textureAtlas);
	}

	public void copySubImageFromProjectToBase(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, textureAtlas, Root.instance.textureAtlas);
	}

	public void copySubImageFromProjectToProject(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, textureAtlas, textureAtlas);
	}

	public void copySubImage(int srcIndex, int destIndex, Texture2D srcAtlas, Texture2D destAtlas)
	{
		if (srcIndex == destIndex)
			return;

//		Debug.Log("copy " + srcIndex + " to " + destIndex);

		int srcX, srcY, destX, destY;
		atlasPixelForIndex(srcIndex, out srcX, out srcY);
		atlasPixelForIndex(destIndex, out destX, out destY);
		Color[] pixels = srcAtlas.GetPixels(srcX, srcY, Root.kSubImageWidth, Root.kSubImageHeight);
		destAtlas.SetPixels(destX, destY, Root.kSubImageWidth, Root.kSubImageHeight, pixels);
		destAtlas.Apply();
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
		currentIndex = 0;

		Texture2D defaultAtlas = Root.instance.textureAtlas;
		textureAtlas = new Texture2D(defaultAtlas.width, defaultAtlas.height);
		textureAtlas.filterMode = FilterMode.Point;
		textureAtlas.SetPixels32(defaultAtlas.GetPixels32());
		textureAtlas.Apply();

		syncMaterialsWithAtlas();
	}

	public void load(ProjectIO projectIO)
	{
		currentIndex = 0;

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
