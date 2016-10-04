using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ThingSubscriber
{
	void onThingAdded(Thing thing);
	void onPrefabChanged(GameObject prefab);
}

public class NotificationManager {

	private List<ThingSubscriber> thingSubscribers;

	public NotificationManager()
	{
		// Find all decendants that wants to know when things change
		thingSubscribers = new List<ThingSubscriber>();
		foreach (ThingSubscriber subscriber in Root.instance.GetComponentsInChildren<ThingSubscriber>())
			thingSubscribers.Add(subscriber);
	}

	public void notifyThingAdded(Thing thing)
	{
		foreach (ThingSubscriber subscriber in thingSubscribers)
			subscriber.onThingAdded(thing);	
	}

	public void notifyPrefabChanged(GameObject prefab)
	{
		foreach (ThingSubscriber subscriber in thingSubscribers)
			subscriber.onPrefabChanged(prefab);	
	}

}
