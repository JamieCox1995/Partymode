using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningThing : MonoBehaviour
{

	private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<Avatar>())
        {
            Avatar avatar = collision.collider.gameObject.GetComponent<Avatar>();

            avatar.ImpactCollision(collision.impulse);
        }
    }
}
