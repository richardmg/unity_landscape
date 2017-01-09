using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour
{
	public GameObject target;
	public float offset = 5;
	
	void Update () {
		Vector3 pos = target.transform.position + (target.transform.forward * offset);
		transform.position = pos;
	}
}
