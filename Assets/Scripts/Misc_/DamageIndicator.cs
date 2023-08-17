using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageIndicator : MonoBehaviour
{
    public TextMeshPro m_Text;

    public Color DamageColor;
    public Color HealColor;

    public void DisplayDamageTaken(float damage)
    {
        if (damage > 0)
        {
            m_Text.color = DamageColor;
        }
        else if (damage < 0)
        {
            m_Text.color = HealColor;
        }
        else if (damage == 0)
        {
            m_Text.color = Color.white;
        }


        m_Text.text = (damage * -1f).ToString("N0") + "HP";

        Invoke("DestroyThis", 1f);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
