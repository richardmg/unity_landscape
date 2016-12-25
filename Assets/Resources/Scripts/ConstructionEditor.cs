using UnityEngine;
using System.Collections;

public class ConstructionEditor : MonoBehaviour {

	EntityInstance m_instance;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setEntityClass(EntityClass entityClass)
	{
		if (m_instance)
			m_instance.hideAndDestroy();

		m_instance = entityClass.createInstance(transform, "ConstructionEntity");
		m_instance.transform.localPosition = Vector3.zero;
		m_instance.makeStandalone(Root.kLodLit);
		m_instance.gameObject.layer = LayerMask.NameToLayer("ConstructionCameraLayer");
	}
}
