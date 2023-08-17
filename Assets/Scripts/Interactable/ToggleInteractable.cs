using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToggleInteractable : Interactable
{
    [Header("Toggle Settings: "),Tooltip("For a ToggleInteractable, we assume that the Primary Action is triggered when the object is switched to the ON state.")]
    public bool isOn;

    public override void InvokePrimaryAction()
    {
        if (m_CurrentState == InteractableState.Disabled) return;

        if (isOn)
        {
            actionToDisplay = actionNames[0];

            if (spawnedCaption != null) spawnedCaption.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Press 'E' to {0}", actionToDisplay);

            onSecondaryActionInvoked.Invoke();
            isOn = false;
        }
        else
        {
            actionToDisplay = actionNames[1];

            if (spawnedCaption != null) spawnedCaption.GetComponentInChildren<TextMeshProUGUI>().text = string.Format("Press 'E' to {0}", actionToDisplay);

            onPrimaryActionInvoked.Invoke();
            isOn = true;
        }
    }

    public override void InvokeSecondaryAction()
    {
        //base.InvokeSecondaryAction();
    }
}
