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

	public void getAtlasPixelForIndex(int atlasIndex, out int x, out int y)
	{
		x = (atlasIndex * Root.kSubImageWidth) % Root.kAtlasWidth;
		y = (int)((atlasIndex * Root.kSubImageWidth) / Root.kAtlasHeight) * Root.kSubImageHeight;
	}

	public Rect getUVRectForIndex(int index)
	{
		float xScale = 1.0f / (float)textureAtlas.width;
		float yScale = 1.0f / (float)textureAtlas.height;

		int x, y;
		getAtlasPixelForIndex(index, out x, out y);

		float uvX1, uvY1, uvX2, uvY2;
		uvX1 = x * xScale;
		uvY1 = y * yScale;
		uvX2 = uvX1 + (Root.kSubImageWidth * xScale);
		uvY2 = uvY1 + (Root.kSubImageHeight * yScale);

		return new Rect(uvX1, uvY1, uvX2 - uvX1, uvY2 - uvY1);
	}

	public void saveBaseAtlasTexture()
	{
		byte[] bytes = Root.instance.textureAtlas.EncodeToPNG();
		string path = Application.dataPath + "/Resources/Textures/textureatlas.png";
		File.WriteAllBytes(path, bytes);
		Root.instance.commandPrompt.log("Wrote texture: " + path);
	}

	public void copySubImageFromBaseToBase(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, Root.instance.textureAtlas, Root.instance.textureAtlas);
	}

	public void copySubImageFromProjectToBase(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, textureAtlas, Root.instance.textureAtlas);
	}

	public void copySubImageFromBaseToProject(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, Root.instance.textureAtlas, textureAtlas);
	}

	public void copySubImageFromProjectToProject(int srcIndex, int destIndex)
	{
		copySubImage(srcIndex, destIndex, textureAtlas, textureAtlas);
	}

	public void copySubImage(int srcIndex, int destIndex, Texture2D srcAtlas, Texture2D destAtlas)
	{
		if (srcIndex == destIndex)
			return;

		int srcX, srcY, destX, destY;
		getAtlasPixelForIndex(srcIndex, out srcX, out srcY);
		getAtlasPixelForIndex(destIndex, out destX, out destY);
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
