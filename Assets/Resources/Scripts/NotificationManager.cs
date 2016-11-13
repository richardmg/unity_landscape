using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IEntityClassListener
{
	void onEntityClassAdded(EntityClass entityClass);
	void onEntityClassRemoved(EntityClass entityClass);
	void onEntityClassChanged(EntityClass entityClass);
}

public interface IEntityInstanceListener
{
	void onEntityInstanceAdded(EntityInstanceDescription entityInstance);
	void onEntityInstanceRemoved(EntityInstanceDescription entityInstance);
	void onEntityInstanceSwapped(EntityInstance from, EntityInstance to);
}

public interface IProjectListener
{
	void onProjectLoaded();
}

public class NotificationManager {

	private List<IEntityClassListener> entityClassListeners = new List<IEntityClassListener>();
	private List<IEntityInstanceListener> entityInstanceListeners = new List<IEntityInstanceListener>();
	private List<IProjectListener> projectListeners = new List<IProjectListener>();

	bool m_postNotifications = false;

	public void addEntityClassListener(IEntityClassListener listener)
	{
		entityClassListeners.Add(listener);
	}

	public void addEntityInstanceListener(IEntityInstanceListener listener)
	{
		entityInstanceListeners.Add(listener);
	}

	public void addProjectListener(IProjectListener listener)
	{
		projectListeners.Add(listener);
	}

	public void notifyEntityInstanceAdded(EntityInstanceDescription desc)
	{
		foreach (IEntityInstanceListener subscriber in entityInstanceListeners)
			subscriber.onEntityInstanceAdded(desc);	
	}

	public void notifyEntityInstanceRemoved(EntityInstanceDescription desc)
	{
		foreach (IEntityInstanceListener subscriber in entityInstanceListeners)
			subscriber.onEntityInstanceRemoved(desc);	
	}

	public void notifyEntityInstanceSwapped(EntityInstance from, EntityInstance to)
	{
		foreach (IEntityInstanceListener subscriber in entityInstanceListeners)
			subscriber.onEntityInstanceSwapped(from, to);	
	}

	public void notifyEntityClassAdded(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (IEntityClassListener subscriber in entityClassListeners)
					subscriber.onEntityClassAdded(entityClass);	
			};
		} else {
			foreach (IEntityClassListener subscriber in entityClassListeners)
				subscriber.onEntityClassAdded(entityClass);	
		}
	}

	public void notifyEntityClassRemoved(EntityClass entityClass, bool postNotification = true)
	{
		if (m_postNotifications) {
			UnityEditor.EditorApplication.delayCall += ()=> {
				foreach (IEntityClassListener subscriber in entityClassListeners)
					subscriber.onEntityClassRemoved(entityClass);	
			};
		} else {
			foreach (IEntityClassListener subscriber in entityClassListeners)
				subscriber.onEntityClassRemoved(entityClass);	
		}
	}

	public void notifyEntityClassChanged(EntityClass entityClass)
	{
		foreach (IEntityClassListener subscriber in entityClassListeners)
			subscriber.onEntityClassChanged(entityClass);	
	}

	public void notifyProjectLoaded()
	{
		foreach (IProjectListener subscriber in projectListeners)
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
