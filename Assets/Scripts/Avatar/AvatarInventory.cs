using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AvatarInventory : MonoBehaviour
{
    [Header("Object Hold Positions")]
    public Transform centralHoldPosition;                // Position in front of the Avatar which the object will be held at.
    public Transform chestHold;
    public Transform leftHandHold;
    public Transform rightHandHold;

    [Space]
    public GameObject heldObject;                       // World object which is being held.
    [SerializeField]private HoldableItem holdableItem; 
    private Animator m_Animator;

    public void PickupObject(GameObject objectToHold)
    {
        heldObject = objectToHold;
        holdableItem = objectToHold.GetComponent<HoldableItem>();

        SetParent();

        heldObject.transform.localPosition = holdableItem.holdOffset;
        heldObject.transform.localRotation = holdableItem.holdRotation;
    }

    private void SetParent()
    {
        switch(holdableItem.holdPosition)
        {
            default:
                heldObject.transform.SetParent(centralHoldPosition);
                break;

            case ObjectHoldPosition.Left:
                heldObject.transform.SetParent(leftHandHold);
                break;

            case ObjectHoldPosition.Right:
                heldObject.transform.SetParent(rightHandHold);
                break;

            case ObjectHoldPosition.Chest:
                heldObject.transform.SetParent(chestHold);
                break;

            case ObjectHoldPosition.Root:
                heldObject.transform.SetParent(transform);
                break;
        }
    }

    public void DropObject()
    {
        if (heldObject == null) return;

        heldObject.transform.parent = null;
        holdableItem.Drop();

        heldObject.GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity);

        heldObject = null;
        holdableItem = null;
    }

    public void SetLeftHandPosition(Transform newHandPosition)
    {
        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        m_Animator.SetIKPosition(AvatarIKGoal.LeftHand, holdableItem.leftHandTransform.position);

        m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        m_Animator.SetIKRotation(AvatarIKGoal.LeftHand, holdableItem.leftHandTransform.rotation);
    }

    public void SetRightHandPosition(Transform newHandPosition)
    {
        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        m_Animator.SetIKPosition(AvatarIKGoal.RightHand, holdableItem.rightHandTransform.position);

        m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        m_Animator.SetIKRotation(AvatarIKGoal.RightHand, holdableItem.rightHandTransform.rotation);
    }

    public void SetBothHandsToRest()
    {
        // Setting the postion and rotation weights to 0.

        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);

        m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        centralHoldPosition.transform.forward = transform.forward;

        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }

        if (heldObject != null)
        {
            if (holdableItem.holdType == ObjectHoldType.LeftHand || holdableItem.holdType == ObjectHoldType.BothHands)
            {
                SetLeftHandPosition(holdableItem.leftHandTransform);
            }

            if (holdableItem.holdType == ObjectHoldType.RightHand || holdableItem.holdType == ObjectHoldType.BothHands)
            {
                SetRightHandPosition(holdableItem.rightHandTransform);
            }
        }
        else
        {
            SetBothHandsToRest();
        }
    }

    public bool HoldingItem
    {
        get
        {
            return heldObject != null;
        }
    }
}


public enum ObjectHoldType
{
    LeftHand, RightHand, BothHands
}

public enum ObjectHoldPosition
{
    Left, Right, Chest, Centre, Root
}
