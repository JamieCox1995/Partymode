using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarHealth : MonoBehaviour
{
    [Header("Avatar Health Settings: ")]
    public float StarterHealth = Mathf.Infinity;
    private float currentHealth = 0f;

    public enum HealthState { Living, Dead };
    public HealthState currentState = HealthState.Living;

    public bool ShowHealth = true;

    [Header("Avatar Health GUI")]
    public TextMeshProUGUI healthTextDisplay;
    public Image healthImageDisplay;
    public Gradient healthColorGradient;
    public Color healthFlashColor;

    public GameObject DamageDisplay;
    public Transform spawnLocation;

    private Avatar _Avatar;

    // Creating the OnDeath event for the Avatar.
    public event Action<Avatar> OnDeath;

    // Use this for initialization
    void Start ()
    {
        _Avatar = GetComponent<Avatar>();

        currentHealth = StarterHealth;
        currentState = HealthState.Living;

        if(ShowHealth) UpdateGUI();
	}

    public void TakeDamage(float damageToTake, bool instantKill = false)
    {
        GameObject indicator = Instantiate(DamageDisplay, spawnLocation.position + (UnityEngine.Random.insideUnitSphere * 0.1f), Quaternion.identity);
        DamageIndicator i = indicator.GetComponent<DamageIndicator>();

        if (instantKill == true)
        {
            i.DisplayDamageTaken(currentHealth);

            currentHealth = 0f;
            Die();

            if(ShowHealth) UpdateGUI();
            currentState = HealthState.Dead;

            return;
        }

        i.DisplayDamageTaken(damageToTake);

        currentHealth -= damageToTake;
        currentHealth = Mathf.Clamp(currentHealth, 0f, StarterHealth);
        if(ShowHealth) UpdateGUI();

        if (currentHealth == 0f)
        {
            Die();
            currentState = HealthState.Dead;

            return;
        }
    }

    public void Die()
    {
        currentState = HealthState.Dead;
        currentHealth = 0f;

        if(ShowHealth) UpdateGUI();

        _Avatar.EnterRagdoll();

        if (OnDeath != null)
        {
            OnDeath(_Avatar);
        }
    }

    private void UpdateGUI()
    {
        float healthRatio = currentHealth / StarterHealth;

        Color col = healthColorGradient.Evaluate(healthRatio);
        healthImageDisplay.color = col;
        healthImageDisplay.fillAmount = healthRatio;

        healthTextDisplay.text = string.Format("{0} HP", currentHealth.ToString("N0"));
    }
}
