using UnityEngine;
using System.Collections;

public class AtlasManager {

	int currentIndex = 0;

	public int acquireIndex()
	{
		return currentIndex++;
	}

	public void releaseIndex(int index)
	{
	}
}
