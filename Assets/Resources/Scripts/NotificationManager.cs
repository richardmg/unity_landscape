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

public interface IEntityInstanceDescriptionListener
{
	void onEntityInstanceDescriptionAdded(EntityInstanceDescription desc);
	void onEntityInstanceDescriptionRemoved(EntityInstanceDescription desc);
	void onEntityInstanceDescriptionChanged(EntityInstanceDescription desc);
}

public interface IEntityInstanceSelectionListener
{
	void onSelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection);
}

public interface IProjectListener
{
	void onProjectLoaded();
}

public class NotificationManager {

	private List<IEntityClassListener> entityClassListeners = new List<IEntityClassListener>();
	private List<IEntityInstanceDescriptionListener> entityInstanceListeners = new List<IEntityInstanceDescriptionListener>();
	private List<IEntityInstanceSelectionListener> entityInstanceSelectionListeners = new List<IEntityInstanceSelectionListener>();
	private List<IProjectListener> projectListeners = new List<IProjectListener>();

	bool m_postNotifications = false;

	public void addEntityClassListener(IEntityClassListener listener)
	{
		entityClassListeners.Add(listener);
	}

	public void addEntityInstanceListener(IEntityInstanceDescriptionListener listener)
	{
		entityInstanceListeners.Add(listener);
	}

	public void addEntitySelectionListener(IEntityInstanceSelectionListener listener)
	{
		entityInstanceSelectionListeners.Add(listener);
	}

	public void removeEntitySelectionListener(IEntityInstanceSelectionListener listener)
	{
		entityInstanceSelectionListeners.Remove(listener);
	}

	public void addProjectListener(IProjectListener listener)
	{
		projectListeners.Add(listener);
	}

	public void notifyEntityInstanceDescriptionAdded(EntityInstanceDescription desc)
	{
		foreach (IEntityInstanceDescriptionListener listener in entityInstanceListeners)
			listener.onEntityInstanceDescriptionAdded(desc);	
	}

	public void notifyEntityInstanceDescriptionRemoved(EntityInstanceDescription desc)
	{
		foreach (IEntityInstanceDescriptionListener listener in entityInstanceListeners)
			listener.onEntityInstanceDescriptionRemoved(desc);	
	}

	public void notifyEntityInstanceDescriptionChanged(EntityInstanceDescription desc)
	{
		foreach (IEntityInstanceDescriptionListener listener in entityInstanceListeners)
			listener.onEntityInstanceDescriptionChanged(desc);
	}

	public void notifyEntityClassAdded(EntityClass entityClass, bool postNotification = true)
	{
//		if (m_postNotifications) {
//			UnityEditor.EditorApplication.delayCall += ()=> {
//				foreach (IEntityClassListener subscriber in entityClassListeners)
//					subscriber.onEntityClassAdded(entityClass);	
//			};
//		} else {
		foreach (IEntityClassListener listener in entityClassListeners)
				listener.onEntityClassAdded(entityClass);	
//		}
	}

	public void notifyEntityClassRemoved(EntityClass entityClass, bool postNotification = true)
	{
//		if (m_postNotifications) {
//			UnityEditor.EditorApplication.delayCall += ()=> {
//				foreach (IEntityClassListener subscriber in entityClassListeners)
//					subscriber.onEntityClassRemoved(entityClass);	
//			};
//		} else {
		foreach (IEntityClassListener listener in entityClassListeners)
				listener.onEntityClassRemoved(entityClass);	
//		}
	}

	public void notifyEntityClassChanged(EntityClass entityClass)
	{
		foreach (IEntityClassListener listener in entityClassListeners)
			listener.onEntityClassChanged(entityClass);	
	}

	public void notifySelectionChanged(List<EntityInstanceDescription> oldSelection, List<EntityInstanceDescription> newSelection)
	{
		List<IEntityInstanceSelectionListener> lockedList = new List<IEntityInstanceSelectionListener>(entityInstanceSelectionListeners);
		foreach (IEntityInstanceSelectionListener listener in lockedList)
			listener.onSelectionChanged(oldSelection, newSelection);
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
