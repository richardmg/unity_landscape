using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface EntityListener
{
	void onEntityInstanceAdded(EntityInstance entityInstance);
	void onEntityClassChanged(EntityClass entityClass);
}

public interface ProjectListener
{
	void onProjectLoaded();
}

public class NotificationManager {

	private List<EntityListener> entityListeners = new List<EntityListener>();
	private List<ProjectListener> projectListeners = new List<ProjectListener>();

	public void addEntityListener(EntityListener listener)
	{
		entityListeners.Add(listener);
	}

	public void addProjectListener(ProjectListener listener)
	{
		projectListeners.Add(listener);
	}

	public void notifyEntityInstanceAdded(EntityInstance entityInstance)
	{
		foreach (EntityListener subscriber in entityListeners)
			subscriber.onEntityInstanceAdded(entityInstance);	
	}

	public void notifyEntityClassChanged(EntityClass entityClass)
	{
		foreach (EntityListener subscriber in entityListeners)
			subscriber.onEntityClassChanged(entityClass);	
	}

	public void notifyProjectLoaded()
	{
		foreach (ProjectListener subscriber in projectListeners)
			subscriber.onProjectLoaded();	
	}

}
