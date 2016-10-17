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

	public void load(FileStream filestream)
	{
	}

	public void save(FileStream filestream)
	{
		int count = allEntityClasses.Count;
		for (int i = 0; i < count; ++i) {
			 allEntityClasses[i].save();
		}
	}
}
