using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOverTime : MonoBehaviour
{

    private Dictionary<AvatarHealth, float> damagingAvatars = new Dictionary<AvatarHealth, float>();


    private void Update()
    {
        AvatarHealth[] avatars = damagingAvatars.Keys.ToArray();

        for(int i = 0; i < avatars.Length; i++)
        {
            float timeIn = damagingAvatars[avatars[i]];

            timeIn += Time.deltaTime;

            if (timeIn >= 1f)
            {
                float damage = Random.Range(5f, 15f);

                avatars[i].TakeDamage(damage);
                timeIn = 0f;
            }

            damagingAvatars[avatars[i]] = timeIn;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        AvatarHealth collidedWith = other.gameObject.GetComponent<AvatarHealth>();

        if (collidedWith == null) return;

        if (damagingAvatars.ContainsKey(collidedWith)) return;

        damagingAvatars.Add(collidedWith, 0f);
    }

    private void OnTriggerExit(Collider other)
    {
        AvatarHealth collidedWith = other.gameObject.GetComponent<AvatarHealth>();

        if (collidedWith == null) return;

        if (damagingAvatars.ContainsKey(collidedWith))
        {
            damagingAvatars.Remove(collidedWith);
        }
    }
}
