using UnityEngine;
using System.Collections;

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

		Debug.Log("copy " + srcIndex + " to " + destIndex);

		int srcX, srcY, destX, destY;
		atlasPixelForIndex(srcIndex, out srcX, out srcY);
		atlasPixelForIndex(destIndex, out destX, out destY);
		Color[] pixels = textureAtlas.GetPixels(srcX, srcY, Root.kSubImageWidth, Root.kSubImageHeight);
		textureAtlas.SetPixels(destX, destY, Root.kSubImageWidth, Root.kSubImageHeight, pixels);
		textureAtlas.Apply();
	}

	public void load(byte[] bytes)
	{
		textureAtlas = new Texture2D(2, 2);
		textureAtlas.LoadImage(bytes);
		Debug.Assert(textureAtlas);
	}

	public byte[] save()
	{
		return textureAtlas.EncodeToPNG();
	}
}
