using UnityEngine;
using System;
using System.Collections;
using System.IO;

public class ProjectIO
{
	public FileStream stream;

	public ProjectIO(FileStream stream)
	{
		this.stream = stream;
	}

	public int readInt()
	{
		byte[] bytes = new byte[sizeof(int)];
		stream.Read(bytes, 0, bytes.Length);
		return BitConverter.ToInt32(bytes, 0);
	}

	public void writeInt(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		stream.Write(bytes, 0, bytes.Length);
	}
}

public class Project
{
	public bool loaded = false;
	public string name;
	public string path;

	public Project(string name)
	{
		this.name = name;
		this.path = Application.persistentDataPath + "/" + name;
	}

	public void save()
	{
		if (!loaded)
			return;
		
		System.IO.Directory.CreateDirectory(path);

		using (FileStream filestream = File.Create(path + "/atlas.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Root.instance.atlasManager.save(projectIO);
		}
		using (FileStream filestream = File.Create(path + "/entities.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Root.instance.entityManager.save(projectIO);
		}

		Debug.Log("Saved project to: " + path);
	}

	public void load()
	{
		using (FileStream filestream = File.OpenRead(path + "/atlas.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Root.instance.atlasManager.load(projectIO);
		}
		using (FileStream filestream = File.OpenRead(path + "/entities.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Root.instance.entityManager.load(projectIO);
		}

		loaded = true;
		Debug.Log("Project loaded from: " + path);
	}
}

public class ProjectManager {
	public Project currentProject;

	public void saveSession()
	{
		// TODO: read from file, and figure out when to call this function
	}

	public void restoreSession()
	{
		// TODO: read from file
		currentProject = new Project("MyWorld");
		currentProject.load();
	}
}
