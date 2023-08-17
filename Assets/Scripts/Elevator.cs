using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Avatar av = other.gameObject.GetComponent<Avatar>();

        if (av == null)
        {
            other.gameObject.transform.SetParent(transform);
        }
        else
        {
            other.transform.parent.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Avatar av = other.gameObject.GetComponent<Avatar>();

        if (av == null)
        {
            other.gameObject.transform.SetParent(null);
        }
        else
        {
            other.transform.parent.SetParent(null);
        }
    }
}
