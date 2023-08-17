using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyer : MonoBehaviour
{
    private ParticleSystem m_Particles;

    // Start is called before the first frame update
    void Start()
    {
        m_Particles = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Particles.main.loop == false && m_Particles.particleCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
