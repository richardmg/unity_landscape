using UnityEngine;
using System.Collections;

//Put this script on your tree objects

public class BillboardScript : MonoBehaviour {

    public GameObject target;

    void Update () 
    {
		transform.LookAt(transform.position + target.transform.rotation * Vector3.forward,
			target.transform.rotation * Vector3.up);
    }
}
