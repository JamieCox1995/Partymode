using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Throw Ability", menuName = "Weapon Abilities/General/Throw", order = 0)]
public class Throw : Ability
{

    [Header("Throw Settings: ")]
    public float ThrowForce = 15f;
    public float SpinSpeed = 100f;

    private HoldableItem m_Item;
    private Rigidbody m_Rigidbody;
    private Camera m_PlayerCamera;

    public override void InitalizeAbility(GameObject parent)
    {
        this.parent = parent;

        m_Rigidbody = parent.GetComponent<Rigidbody>();
        m_Item = parent.GetComponent<HoldableItem>();

        m_PlayerCamera = Camera.main;
    }

    public override void OnItemPickedUp()
    {
        isReady = true;
        cooldownTimer = CooldownTime;
    }

    public override void ReadyAbility()
    {
        Debug.Log("Preparing Throw");
    }

    public override void TriggerAbility()
    {
        if (isReady)
        {
            // We want to create a force in the direction we are looking.
            Vector3 throwForce = m_PlayerCamera.transform.forward * ThrowForce;

            // We now want to tell our holding object to drop itself.
            m_Item.m_HoldingAvatar.DropObject();

            m_Rigidbody.AddForce(throwForce, ForceMode.Impulse);
            m_Rigidbody.AddRelativeTorque(SpinSpeed, 0f, 0f);

            isReady = false;
            cooldownTimer = 0f;
        }
    }

    public override void UpdateAbility(float deltaTime)
    {
        cooldownTimer += Time.deltaTime;

        if(cooldownTimer >= CooldownTime)
        {
            cooldownTimer = CooldownTime;

            isReady = true;
        }
    }
}
