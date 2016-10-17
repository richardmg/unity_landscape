using UnityEngine;
using System.Collections;
using System.IO;

public class ProjectManager {
	string filename;

	public void saveProject()
	{
		saveProjectAs(filename);
	}

	public void saveProjectAs(string filename)
	{
		this.filename = filename;
		byte[] atlasPng = Root.instance.textureAtlas.EncodeToPNG();
		Debug.Log("save png to: " + Application.persistentDataPath);
//		File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
	}

	public void loadProject(string filename)
	{
	}
}
