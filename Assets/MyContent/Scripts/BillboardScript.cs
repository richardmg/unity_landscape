using UnityEngine;
using System.Collections;

//Put this script on your tree objects

public class BillboardScript : MonoBehaviour {

    public GameObject target;        //The target of the rotation (An example would be your camera)
    public int rotSpeed = 1;        //How quickly the trees rotate
    private Transform myTransform;  //Current object's transform

    void Awake()
    {
       myTransform = transform;
    }

    void Update () 
    {
        //Look at Player on the X and Z axis
        myTransform.rotation = Quaternion.Slerp(myTransform.rotation, Quaternion.LookRotation(new Vector3(target.transform.position.x, 0, target.transform.position.z) - new Vector3(myTransform.position.x, 0, myTransform.position.z)), rotSpeed * Time.deltaTime);
    }
}
