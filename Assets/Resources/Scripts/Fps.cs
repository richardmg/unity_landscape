using UnityEngine;
using System.Collections;

public class Fps : MonoBehaviour
{
	float deltaTime = 0.0f;

	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;

		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float fps = 1.0f / deltaTime;
		string text = string.Format("Top level: {0}, Cache size: {1}, FPS: {2:0.}", VoxelObject.voxelObjectCount, Root.instance.meshCache.size(), fps);
		GUI.Label(rect, text, style);
	}
}