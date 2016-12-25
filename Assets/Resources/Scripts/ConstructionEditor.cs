using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConstructionEditor : MonoBehaviour {

	public GameObject constructionCameraGO;
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

	public void onZoomSliderChanged(Slider slider)
	{
		Vector3 cameraPos = new Vector3(0, 0, slider.normalizedValue * -200);
		constructionCameraGO.transform.localPosition = cameraPos;
	}
}
