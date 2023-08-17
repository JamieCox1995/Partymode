using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoldableItem : Interactable
{
    [Header("Object Hold Settings: ")]
    public Vector3 holdOffset;
    public Quaternion holdRotation;
    protected bool beingHeld = false;

    [Space, Tooltip("Defines which position the Avatar will hold the object.")]
    public ObjectHoldPosition holdPosition = ObjectHoldPosition.Centre;
    [Tooltip("Defines if the Avatar should visually hold the item with 1 or boths hands.")]
    public ObjectHoldType holdType = ObjectHoldType.BothHands;
    public Transform leftHandTransform, rightHandTransform;

    [Header("Abilities: ")]
    public List<ObjectAbilityInput> objectAbilities = new List<ObjectAbilityInput>();

    [HideInInspector] public AvatarInventory m_HoldingAvatar;
    private Rigidbody m_Rigidbody;
    private Collider m_Collider;
    private Animator m_Animator;

    protected override void Start()
    {
        base.Start();

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_Animator = GetComponent<Animator>();

        foreach(ObjectAbilityInput ability in objectAbilities)
        {
            ability.Ability.InitalizeAbility(gameObject);
        }

        if(m_Animator)
        {
            m_Animator.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (beingHeld)
        {
            // In the update loop for a holdable object, we want to check to see if the player has tried to trigger any abilities.
            foreach(ObjectAbilityInput ability in objectAbilities)
            {
                ability.Ability.UpdateAbility(Time.deltaTime);
            }

            CheckActionInputs();
        }
    }

    public void CheckActionInputs()
    {
        foreach(ObjectAbilityInput ability in objectAbilities)
        {
            if (ability.RequiresKeyHeldDown && Input.GetButton(ability.InitiatorAction))
            {
                ability.Ability.ReadyAbility();

                if (Input.GetButtonDown(ability.PrimaryActionName))
                {
                    ability.Ability.TriggerAbility();
                }
            }
            else
            {
                if (Input.GetButtonDown(ability.PrimaryActionName) && ability.RequiresKeyHeldDown == false)
                {
                    ability.Ability.TriggerAbility();
                }
            }
        }
    }

    public void Pickup()
    {
        // We also want to make this kinematic and not use gravity
        m_Rigidbody.useGravity = false;
        m_Rigidbody.isKinematic = true;

        m_Collider.enabled = false;

        // When an object is picked up, we want to set to get the Inventory on the Avatar and
        // Set it as the Picked Up Object.
        AvatarInventory avatarInventory = Target.GetComponent<AvatarInventory>();
        m_HoldingAvatar = avatarInventory;

        avatarInventory.PickupObject(gameObject);

        foreach(ObjectAbilityInput ab in objectAbilities)
        {
            ab.Ability.OnItemPickedUp();
        }

        if (m_Animator)
        {
            m_Animator.enabled = true;
        }

        beingHeld = true;
    }

    public void Drop()
    {
        m_Collider.enabled = true;
        m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;

        if (m_Animator)
        {
            m_Animator.enabled = false;
        }

        m_HoldingAvatar = null;

        beingHeld = false;
    }
}

[System.Serializable]
public class ObjectAbilityInput
{
    public string AbilityName;

    public string PrimaryActionName;

    public bool RequiresKeyHeldDown = false;
    public string InitiatorAction;

    public Ability Ability;
}
