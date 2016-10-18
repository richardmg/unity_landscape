using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EntityManager
{
	public List<EntityClass> allEntityClasses;

	public void addEntityClass(EntityClass entityClass)
	{
		entityClass.id = allEntityClasses.Count;
		allEntityClasses.Add(entityClass);
	}

	public EntityClass getEntity(int id)
	{
		return allEntityClasses[id];
	}

	public void initNewProject()
	{
		allEntityClasses = new List<EntityClass>();

		// Add premade entity classes already present in the texture atlas
		new EntityClass("SquareTree");
	}

	public void load(ProjectIO projectIO)
	{
		allEntityClasses = new List<EntityClass>();
		int classCount = projectIO.readInt();

		for (int i = 0; i < classCount; ++i)
			EntityClass.load(projectIO);
	}

	public void save(ProjectIO projectIO)
	{
		int classCount = allEntityClasses.Count;
		projectIO.writeInt(classCount);

		for (int i = 0; i < classCount; ++i)
			allEntityClasses[i].save(projectIO);
	}
}
