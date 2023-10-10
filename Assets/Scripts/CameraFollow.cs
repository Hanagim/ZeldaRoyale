using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Transform target;
    Vector3 targetVector3;

    // Update is called once per frame
    void LateUpdate()
    {
        targetVector3.x = target.transform.position.x;
        targetVector3.y = 5.0f;
        targetVector3.z= target.transform.position.z - 5.0f;
        transform.position = targetVector3;
    }
}
