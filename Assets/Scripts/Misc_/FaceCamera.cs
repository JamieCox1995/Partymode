using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{

    private void Update()
    {
        Vector3 directionTo = Camera.main.transform.position - transform.position;

        directionTo.x = directionTo.z = 0f;

        transform.LookAt(Camera.main.transform.position - directionTo);
        transform.rotation = Camera.main.transform.rotation;
    }
}
