﻿using UnityEngine;
using System.Collections;
using System.IO;

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
			Root.instance.atlasManager.save(filestream);
		}
		using (FileStream filestream = File.Create(path + "/entities.dat"))
		{
			Root.instance.entityManager.save(filestream);
		}

		Debug.Log("Saved project to: " + path);
	}

	public void load()
	{
		using (FileStream filestream = File.OpenRead(path + "/atlas.dat"))
		{
			Root.instance.atlasManager.load(filestream);
		}
		using (FileStream filestream = File.OpenRead(path + "/entities.dat"))
		{
			Root.instance.entityManager.load(filestream);
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
