using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : ToggleInteractable
{
    [Header("Door Settings: ")]
    public bool isLocked = false;
    public Animator m_Animator;

    public void SetDoorState(bool state)
    {
        if (isLocked == false)
        {
            m_Animator.SetBool("Opened", state);
        }
    }

    public void LockDoor(bool lockState)
    {
        isLocked = lockState;
    }
}
