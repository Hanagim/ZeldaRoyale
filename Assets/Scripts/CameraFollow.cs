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
        if (target == null)
        {

            var playersToFollow = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in playersToFollow)
            {
                if (player.GetComponent<Character>().IsPlayer)
                {
                    target = player.gameObject.transform;
                }
            }
        }

        if (target == null)
        {
            return;
        }

        targetVector3.x = target.transform.position.x;
        targetVector3.y = target.transform.position.y + 6.0f;
        targetVector3.z= target.transform.position.z - 10.0f;
        transform.position = targetVector3;
    }
}
