using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface EntityClassListener
{
	void onEntityClassAdded(EntityClass entityClass);
	void onEntityClassRemoved(EntityClass entityClass);
	void onEntityClassChanged(EntityClass entityClass);
}

public interface EntityInstanceListener
{
	void onEntityInstanceAdded(EntityInstance entityInstance);
	void onEntityInstanceSwapped(EntityInstance from, EntityInstance to);
}

public interface ProjectListener
{
	void onProjectLoaded();
}

public class NotificationManager {

	private List<EntityClassListener> entityClassListeners = new List<EntityClassListener>();
	private List<EntityInstanceListener> entityInstanceListeners = new List<EntityInstanceListener>();
	private List<ProjectListener> projectListeners = new List<ProjectListener>();

	bool m_postNotifications = false;

	public void addEntityClassListener(EntityClassListener listener)
	{
		entityClassListeners.Add(listener);
	}

	public void addEntityInstanceListener(EntityInstanceListener listener)
	{
		entityInstanceListeners.Add(listener);
	}

	public void addProjectListener(ProjectListener listener)
	{
		projectListeners.Add(listener);
	}

	public void notifyEntityInstanceAdded(EntityInstance entityInstance)
	{
		foreach (EntityInstanceListener subscriber in entityInstanceListeners)
			subscriber.onEntityInstanceAdded(entityInstance);	
	}

	public void notifyEntityInstanceSwapped(EntityInstance from, EntityInstance to)
	{
		foreach (EntityInstanceListener subscriber in entityInstanceListeners)
			subscriber.onEntityInstanceSwapped(from, to);	
	}

	public void notifyEntityClassAdded(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (EntityClassListener subscriber in entityClassListeners)
					subscriber.onEntityClassAdded(entityClass);	
			};
		} else {
			foreach (EntityClassListener subscriber in entityClassListeners)
				subscriber.onEntityClassAdded(entityClass);	
		}
	}

	public void notifyEntityClassRemoved(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (EntityClassListener subscriber in entityClassListeners)
					subscriber.onEntityClassRemoved(entityClass);	
			};
		} else {
			foreach (EntityClassListener subscriber in entityClassListeners)
				subscriber.onEntityClassRemoved(entityClass);	
		}
	}

	public void notifyEntityClassChanged(EntityClass entityClass)
	{
		foreach (EntityClassListener subscriber in entityClassListeners)
			subscriber.onEntityClassChanged(entityClass);	
	}

	public void notifyProjectLoaded()
	{
		foreach (ProjectListener subscriber in projectListeners)
			subscriber.onProjectLoaded();	
	}

	public void postNotificationBlock(bool post, Action function)
	{
		bool prevPost = m_postNotifications;
		m_postNotifications = post;
		function();
		m_postNotifications = prevPost;
	}

}
