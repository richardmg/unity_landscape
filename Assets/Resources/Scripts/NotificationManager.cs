using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface EntityListener
{
	void onEntityInstanceAdded(EntityInstance entityInstance);
	void onEntityClassAdded(EntityClass entityClass);
	void onEntityClassRemoved(EntityClass entityClass);
	void onEntityClassChanged(EntityClass entityClass);
}

public interface ProjectListener
{
	void onProjectLoaded();
}

public class NotificationManager {

	private List<EntityListener> entityListeners = new List<EntityListener>();
	private List<ProjectListener> projectListeners = new List<ProjectListener>();

	bool m_postNotifications = false;

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

	public void notifyEntityClassAdded(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (EntityListener subscriber in entityListeners)
					subscriber.onEntityClassAdded(entityClass);	
			};
		} else {
			foreach (EntityListener subscriber in entityListeners)
				subscriber.onEntityClassAdded(entityClass);	
		}
	}

	public void notifyEntityClassRemoved(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (EntityListener subscriber in entityListeners)
					subscriber.onEntityClassRemoved(entityClass);	
			};
		} else {
			foreach (EntityListener subscriber in entityListeners)
				subscriber.onEntityClassRemoved(entityClass);	
		}
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

	public void postNotificationBlock(bool post, Action function)
	{
		bool prevPost = m_postNotifications;
		m_postNotifications = post;
		function();
		m_postNotifications = prevPost;
	}

}
