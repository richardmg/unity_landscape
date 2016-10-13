using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface EntitySubscriber
{
	void onEntityInstanceAdded(GameObject entityInstance);
	void onEntityClassChanged(EntityClass entityClass);
}

public class NotificationManager {

	private List<EntitySubscriber> entitySubscribers;

	public NotificationManager()
	{
		// Find all decendants that wants to know when things change
		entitySubscribers = new List<EntitySubscriber>();
		foreach (EntitySubscriber subscriber in Root.instance.GetComponentsInChildren<EntitySubscriber>())
			entitySubscribers.Add(subscriber);
	}

	public void notifyEntityInstanceAdded(GameObject entityInstance)
	{
		foreach (EntitySubscriber subscriber in entitySubscribers)
			subscriber.onEntityInstanceAdded(entityInstance);	
	}

	public void notifyEntityClassChanged(EntityClass entityClass)
	{
		foreach (EntitySubscriber subscriber in entitySubscribers)
			subscriber.onEntityClassChanged(entityClass);	
	}

}
