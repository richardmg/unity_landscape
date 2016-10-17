using UnityEngine;
using System;
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
		return;


		int classCount = allEntityClasses.Count;
		byte[] classCountBytes = BitConverter.GetBytes(classCount);
		filestream.Write(classCountBytes, 0, classCountBytes.Length);

		for (int i = 0; i < classCount; ++i)
			allEntityClasses[i].save(filestream);
	}
}
