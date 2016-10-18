using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public interface IProjectIOMember
{
	void initNewProject();
	void save(ProjectIO projectIO);
	void load(ProjectIO projectIO);
}

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

	static List<IProjectIOMember> projectIOMemberList;

	public Project(string name)
	{
		if (projectIOMemberList == null)
			Project.initProjectIOMemberList();

		this.name = name;
		this.path = Application.persistentDataPath + "/" + name;
	}

	static public void initProjectIOMemberList()
	{
		projectIOMemberList = new List<IProjectIOMember>();
		projectIOMemberList.Add(Root.instance.atlasManager);
		projectIOMemberList.Add(Root.instance.entityManager);
		projectIOMemberList.Add(Root.instance.player);
	}

	public bool exists()
	{
		return Directory.Exists(path);
	}

	public void initNewProject()
	{
		foreach (IProjectIOMember member in projectIOMemberList)
			member.initNewProject();

		Debug.Log("Project created: " + name);
	}

	public void save()
	{
		System.IO.Directory.CreateDirectory(path);

		using (FileStream filestream = File.Create(path + "/savegame.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			projectIO.writeInt(fileVersion);
			foreach (IProjectIOMember member in projectIOMemberList)
				member.save(projectIO);
		}

		Debug.Log("Project saved: " + path);
	}

	public void load()
	{
		using (FileStream filestream = File.OpenRead(path + "/savegame.dat"))
		{
			ProjectIO projectIO = new ProjectIO(filestream);
			Debug.Assert(fileVersion == projectIO.readInt());
			foreach (IProjectIOMember member in projectIOMemberList)
				member.load(projectIO);
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
		currentProject.initNewProject();
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
