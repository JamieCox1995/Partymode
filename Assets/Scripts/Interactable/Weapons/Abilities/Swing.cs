using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Swing Ability", menuName = "Weapon Abilities/Sword/Swing", order = 0)]
public class Swing : Ability
{
    public override void InitalizeAbility(GameObject parent)
    {
        isReady = true;
    }

    public override void OnItemPickedUp()
    {
        isReady = true;

        cooldownTimer = CooldownTime;
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

    public override void ReadyAbility()
    {

    }

    public override void TriggerAbility()
    {
        if(isReady)
        {
            isReady = false;
            cooldownTimer = 0f;

            Debug.Log("Shhhwing");
        }
    }
}
