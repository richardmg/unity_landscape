using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EntityManager : IProjectIOMember
{
	public List<EntityClass> allEntityClasses;

	public void addEntityClass(EntityClass entityClass, bool notify = true)
	{
		entityClass.id = allEntityClasses.Count;
		allEntityClasses.Add(entityClass);

		if (notify)
			Root.instance.notificationManager.notifyEntityClassAdded(entityClass);
	}

	public void removeEntityClass(EntityClass entityClass, bool notify = true)
	{
		allEntityClasses.Remove(entityClass);

		if (notify)
			Root.instance.notificationManager.notifyEntityClassAdded(entityClass);
	}

	public EntityClass getEntity(int id)
	{
		if (id < 0 || id >= allEntityClasses.Count)
			return null;
		return allEntityClasses[id];
	}

	public void initNewProject()
	{
		allEntityClasses = new List<EntityClass>();

		// Add premade entity classes already present in the texture atlas
		new EntityClass("BallTree");
	}

	public void load(ProjectIO projectIO)
	{
		allEntityClasses = new List<EntityClass>();
		int classCount = projectIO.readInt();

		for (int i = 0; i < classCount; ++i) {
			EntityClass entityClass = new EntityClass(false);
			entityClass.load(projectIO);
		}
	}

	public void save(ProjectIO projectIO)
	{
		int classCount = allEntityClasses.Count;
		projectIO.writeInt(classCount);

		for (int i = 0; i < classCount; ++i)
			allEntityClasses[i].save(projectIO);
	}
}
