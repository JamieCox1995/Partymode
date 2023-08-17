using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Interactable : MonoBehaviour
{
    [System.Serializable] public enum InteractableState { Disabled, Enabled, Working }
    public InteractableState m_CurrentState = InteractableState.Enabled;

    [Header("Action Strings:")]
    public string[] actionNames = new string[2];
    protected string actionToDisplay;

    [Space, Header("Event Handlers:")]
    public UnityEvent onPrimaryActionInvoked;
    public UnityEvent onSecondaryActionInvoked;

    [Header("Highlight Settings:")]
    public bool showCaption = true;
    public GameObject highlightCaption;
    public Avatar Target;
    private float colorLerpTime = 0.75f;

    [ColorUsage(true, true)]public Color activeHightlightedColor;
    [ColorUsage(true, true)]public Color disabledHighlightedColor;
    [ColorUsage(true, true)]private Color currentHighlightColor;

    public Vector3 captionPositionOffset = new Vector3(0f, -0.5f, 0.25f);
    protected GameObject spawnedCaption;

    protected virtual void Start()
    {
        actionToDisplay = actionNames[0];
    }

    protected virtual void Update()
    {
        if (spawnedCaption != null)
        {
            spawnedCaption.transform.position = transform.position + captionPositionOffset;
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = rot.z = 0f;

            spawnedCaption.transform.eulerAngles = rot;
        }
    }

    public virtual void ObjectHighlighted(Avatar target)
    {
        // First of all we want to spawn in the caption text if none has been spawned.
        if (spawnedCaption == null && showCaption == true)
        {
            spawnedCaption = Instantiate(highlightCaption, transform.position + captionPositionOffset, Quaternion.identity);
            Vector3 rot = transform.rotation.eulerAngles;
            rot.x = rot.z = 0f;

            spawnedCaption.transform.eulerAngles = rot;

            // As we only want to set the text on the caption once, we want to get the TextMeshPro Text here and set it.
            TextMeshProUGUI textMesh = spawnedCaption.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.text = string.Format("Press 'E' to {0}", actionToDisplay);
        }

        Target = target;

        StopAllCoroutines();
        Color color = (m_CurrentState == InteractableState.Disabled) ? disabledHighlightedColor : activeHightlightedColor;
        StartCoroutine(LerpHighlightColor(color));
    }

    public virtual void UnHighlightObject()
    {
        if (spawnedCaption != null)
        {
            Destroy(spawnedCaption);
        }

        Target = null;

        StopAllCoroutines();

        Color col = currentHighlightColor;
        col.a = 0f;

        StartCoroutine(LerpHighlightColor(col));
    }

    public virtual void InvokePrimaryAction()
    {
        if (m_CurrentState == InteractableState.Disabled) return;

        onPrimaryActionInvoked.Invoke();
    }

    public virtual void InvokeSecondaryAction()
    {
        if (m_CurrentState == InteractableState.Disabled) return;
    }

    protected IEnumerator LerpHighlightColor(Color newColor)
    {
        if (GetComponent<Renderer>() != null)
        {
            Material material = GetComponent<Renderer>().material;

            float timer = 0f;

            while (timer < colorLerpTime)
            {
                timer += Time.deltaTime;

                currentHighlightColor = Color.Lerp(currentHighlightColor, newColor, timer / colorLerpTime);

                material.SetColor("_OutlineColor", currentHighlightColor);

                yield return null;
            }
        }
    }
}
