using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantDie : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        AvatarHealth avatar = other.gameObject.GetComponent<AvatarHealth>();

        if (avatar != null)
        {
            avatar.Die();
        }
    }
}
