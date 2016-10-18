using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;

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

	public void writeString(string str)
	{
		byte[] bytes = Encoding.Unicode.GetBytes(str);
		writeInt(bytes.Length);
		stream.Write(bytes, 0, bytes.Length);
	}

	public string readString()
	{
		int length = readInt();
		byte[] bytes = new byte[length];
		stream.Read(bytes, 0, bytes.Length);
		return Encoding.Unicode.GetString(bytes);
	}

}

public class Project
{
	public string name;
	public string path;

	int fileVersion = 1;

	public Project(string name)
	{
		this.name = name;
		this.path = Application.persistentDataPath + "/" + name;
	}

	public bool exists()
	{
		return Directory.Exists(path);
	}

	public void initAsNewProject()
	{
		Root.instance.atlasManager.initNewProject();
		Root.instance.entityManager.initNewProject();
		Root.instance.player.initNewProject();

		Debug.Log("Project created: " + name);
	}

	public void save()
	{
		System.IO.Directory.CreateDirectory(path);

		using (FileStream filestream = File.Create(path + "/savegame.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			projectIO.writeInt(fileVersion);
			Root.instance.atlasManager.save(projectIO);
			Root.instance.entityManager.save(projectIO);
			Root.instance.player.save(projectIO);
		}

		Debug.Log("Project saved: " + path);
	}

	public void load()
	{
		using (FileStream filestream = File.OpenRead(path + "/savegame.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Debug.Assert(fileVersion == projectIO.readInt());
			Root.instance.atlasManager.load(projectIO);
			Root.instance.entityManager.load(projectIO);
			Root.instance.player.load(projectIO);
		}

		Debug.Log("Project loaded: " + path);
	}
}

public class ProjectManager {
	public Project currentProject;

	public bool createNewProject(string projectName, bool overwrite = false)
	{
		Project newProject = new Project(projectName);
		if (!overwrite && newProject.exists()) {
			Debug.Log("Another project with name '" + projectName + "' already exists!");
			return false;
		}

		currentProject = newProject;
		currentProject.initAsNewProject();
		return true;
	}

	public void saveSession()
	{
		// TODO: read from file, and figure out when to call this function
	}

	public void restoreSession()
	{
		// TODO: read from file
		string projectName = "MyWorld2";

//		createNewProject(projectName, true);
//		return;

		currentProject = new Project(projectName);
		if (currentProject.exists()) {
			currentProject.load();
		} else {
			Debug.Log("Could not open last project: " + projectName);
			createNewProject(projectName);
		}
	}
}
