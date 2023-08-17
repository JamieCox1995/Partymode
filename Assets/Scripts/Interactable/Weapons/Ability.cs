using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public float CooldownTime = 1f;
    protected float cooldownTimer = 0f;
    protected bool isReady = true;

    protected GameObject parent;

    public abstract void InitalizeAbility(GameObject parent);

    public abstract void OnItemPickedUp();

    public abstract void UpdateAbility(float deltaTime);

    public abstract void ReadyAbility();

    public abstract void TriggerAbility();
}