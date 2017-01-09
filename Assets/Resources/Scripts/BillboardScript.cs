using UnityEngine;
using System.Collections;

//Put this script on your tree objects

public class BillboardScript : MonoBehaviour
{
    public GameObject target;

	Vector3 m_targetPos = Vector3.zero;

    void Update () 
    {
//		if (target.transform.position == m_targetPos)
//			return;
//
//		m_targetPos = target.transform.position;
		transform.LookAt(transform.position + target.transform.rotation * Vector3.forward,
			target.transform.rotation * Vector3.up);
    }
}
