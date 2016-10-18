using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class EntityManager
{
	public List<EntityClass> allEntityClasses = new List<EntityClass>();

	public void addEntityClass(EntityClass entityClass)
	{
		allEntityClasses.Add(entityClass);
	}

	public void load(ProjectIO projectIO)
	{
	}

	public void save(ProjectIO projectIO)
	{
		return;

		int classCount = allEntityClasses.Count;
		projectIO.writeInt(classCount);

		for (int i = 0; i < classCount; ++i)
			allEntityClasses[i].save(projectIO);
	}
}
