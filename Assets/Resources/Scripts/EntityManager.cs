using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EntityManager
{
	public List<EntityClass> allEntityClasses = new List<EntityClass>();

	public void addEntityClass(EntityClass entityClass)
	{
		allEntityClasses.Add(entityClass);
	}
}
