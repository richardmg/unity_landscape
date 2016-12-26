﻿using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EntityClassManager : IProjectIOMember
{
	public List<EntityClass> allEntityClasses = new List<EntityClass>();

	const string kEntityPrefabFolder = "Prefabs/EntityClassPrefabs";

	public void addEntityClass(EntityClass entityClass, bool notify = true)
	{
		int newID = allEntityClasses.Count;
		addEntityClass(entityClass, newID, notify);
	}

	public void addEntityClass(EntityClass entityClass, int id, bool notify = true)
	{
		entityClass.id = id;
		allEntityClasses.Add(entityClass);

		if (notify)
			Root.instance.notificationManager.notifyEntityClassAdded(entityClass);
	}

	public void removeEntityClass(EntityClass entityClass, bool notify = true)
	{
		entityClass.removed = true;
		allEntityClasses.Remove(entityClass);

		if (notify)
			Root.instance.notificationManager.notifyEntityClassRemoved(entityClass);
	}

	public void removeAllEntityClasses()
	{
		// Mark the old classes as destroyed, since they should
		// no longer be used by anyone.
		foreach (EntityClass entityClass in allEntityClasses)
			entityClass.removed = true;

		allEntityClasses = new List<EntityClass>();
	}

	public EntityClass getEntity(int id)
	{
		if (id < 0 || id >= allEntityClasses.Count)
			return null;
		return allEntityClasses[id];
	}

	public void initNewProject()
	{
		removeAllEntityClasses();
	}

	public void load(ProjectIO projectIO)
	{
		removeAllEntityClasses();

		int classCount = projectIO.readInt();
		for (int i = 0; i < classCount; ++i)
			EntityClass.load(projectIO, false);
	}

	public void save(ProjectIO projectIO)
	{
		int classCount = allEntityClasses.Count;
		projectIO.writeInt(classCount);

		for (int i = 0; i < classCount; ++i)
			allEntityClasses[i].save(projectIO);
	}
}
