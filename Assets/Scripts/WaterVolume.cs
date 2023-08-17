using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterVolume : MonoBehaviour
{
    public enum VolumeType { Plain, Contained }
    [Header("General Water Settings:")]
    public VolumeType m_VolumeType;
    public float waterLevel = 0f;

    [Header("Water Buoyancy:")]
    public bool allowBuoyancy = false;
    public float buoyancyForce;
    public float waterDrag = 0.4f;

    [Header("Avatar Drowning Settings:")]
    public float timeToDrown = 5f;
    public float damageFromDrowning = 15f;

    public Dictionary<Avatar, float> wetBois = new Dictionary<Avatar, float>();     // This should probably have a more professional name, but oh well. 
    private Collider m_Collider;

    public bool KillSubmergedAvatars = true;

    private void Start()
    {
        m_Collider = GetComponent<Collider>();
    }

    public void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 velocity = rb.velocity;

            rb.velocity = velocity / 5f;
        }


        Avatar newWetBoi = other.gameObject.GetComponent<Avatar>();

        if (newWetBoi == null) return;

        if (wetBois.ContainsKey(newWetBoi))
        {
            return;
        }

        //newWetBoi.InWater = true;

        wetBois.Add(newWetBoi, 0f);


    }

    public void OnTriggerStay(Collider other)
    {
        if (allowBuoyancy)
        {
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Avatar avatar = other.gameObject.GetComponent<Avatar>();

            if (rb == null)
            {
                return;
            }

            rb.drag = waterDrag /* (depth / maxDepth)*/;

            //if (avatar == null)
            //{
                // In here we are going to add a force to the object based on how deep they are in the water volume.
                // We can assume that "transform.position.y" can be used as the water level as we always move the collider down.
                float maxDepth = GetComponent<Collider>().bounds.size.y;
                float depth = transform.position.y - other.gameObject.transform.position.y;

                float area = other.bounds.size.x * other.bounds.size.z;

                float forceToAdd = (buoyancyForce * area) * depth;

                rb.AddForce(forceToAdd * Vector3.up, ForceMode.Force);
            /*}

            if (avatar != null)
            {
                float area = other.bounds.size.x * other.bounds.size.z;

                float forceToAdd = (buoyancyForce * area * 3f);

                rb.AddForce(forceToAdd * Vector3.up, ForceMode.Force);
            }*/

        }
    }

    public void OnTriggerExit(Collider other)
    {
        Avatar noLongerWetBoi = other.gameObject.GetComponent<Avatar>();

        if (noLongerWetBoi == null) return;

        if (wetBois.ContainsKey(noLongerWetBoi))
        {
            //noLongerWetBoi.InWater = false;
            wetBois.Remove(noLongerWetBoi);
        }
    }

    private void Update()
    {
        // We want to get all of the Avatars and check to see if we should damage them.
        Avatar[] allWetBois = wetBois.Keys.ToArray();

        for(int i = 0; i < allWetBois.Length; i++)
        {
            // Checking to see if their head is submerged under water.
            if (AvatarSubmerged(allWetBois[i]) && allWetBois[i].m_AvatarHealth.currentState != AvatarHealth.HealthState.Dead)
            {
                if (KillSubmergedAvatars) allWetBois[i].m_AvatarHealth.TakeDamage(1, true);

                Debug.Log("We're fully submerged and will start taking damage.");

                float currentSubmergedTime = wetBois[allWetBois[i]];

                currentSubmergedTime += Time.deltaTime;

                if (currentSubmergedTime >= timeToDrown)
                {
                    allWetBois[i].m_AvatarHealth.TakeDamage(damageFromDrowning);
                    currentSubmergedTime = 0f;
                }

                wetBois[allWetBois[i]] = currentSubmergedTime;
            }
        }
    }

    private bool AvatarSubmerged(Avatar avatar)
    {
        if (m_VolumeType == VolumeType.Plain)
        {
            if (avatar.m_Head.transform.position.y <= waterLevel)
            {
                return true;
            }
        }
        else if (m_VolumeType == VolumeType.Contained)
        {
            if (m_Collider.bounds.Contains(avatar.m_Head.transform.position))
            {
                return true;
            }
        }

        return false;
    }
}
