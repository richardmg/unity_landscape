using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Global {
	public static List<PrefabVariant> prefabVariants;

	public static readonly int kAtlasWidth = 64;
	public static readonly int kAtlasHeight = 64;
	public static readonly int kSubImageWidth = 16;
	public static readonly int kSubImageHeight = 8;

	public static void atlasPixelForIndex(int atlasIndex, out int x, out int y)
	{
		x = (atlasIndex * Global.kSubImageWidth) % Global.kAtlasWidth;
		y = (int)((atlasIndex * Global.kSubImageWidth) / Global.kAtlasHeight) * Global.kSubImageHeight;
	}

	public static GameObject getPrefab(string prefabName)
    {
		return (GameObject)Resources.Load("Prefabs/" + prefabName, typeof(GameObject));
    }
}
