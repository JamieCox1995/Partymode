using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AvatarAIControl : MonoBehaviour
{
    private Avatar m_Avatar;
    private NavMeshAgent m_NavAgent;

    public Vector3 TargetLocation = new Vector3(-10f, 0f, -18f);
    private bool fleeingAggressor = false;
    private GameObject Aggressor;
    public float AggressorFleeDistance;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();

        m_NavAgent.SetDestination(TargetLocation);
    }

    private void Initialize()
    {
        m_Avatar = GetComponent<Avatar>();
        m_NavAgent = GetComponent<NavMeshAgent>();

        m_NavAgent.updatePosition = false;
        m_NavAgent.speed = m_Avatar.MovementSpeed * m_Avatar.WalkingSpeedFactor;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveLocation = TargetLocation;

        SetNavMeshAgentDestination(moveLocation);

        m_NavAgent.speed = m_Avatar.MovementSpeed * m_Avatar.WalkingSpeedFactor;

        m_Avatar.UpdateSpeedFactor(m_Avatar.WalkingSpeedFactor);

        m_Avatar.Move(m_NavAgent.velocity.normalized, 1f);

        transform.position = m_NavAgent.nextPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Potential Agressor in range?");

        Avatar aggressor = other.GetComponent<Avatar>();

        if (aggressor != null)
        {
            Aggressor = aggressor.gameObject;

            fleeingAggressor = true;
        }
    }

    private void SetNavMeshAgentDestination(Vector3 target)
    {
        if (m_NavAgent.destination != TargetLocation)
        {
            m_NavAgent.SetDestination(target);
        }
    }

    private Vector3 GenerateFleeVector()
    {
        Vector3 fleeVector = new Vector3();

        fleeVector = (transform.position - Aggressor.transform.position).normalized * AggressorFleeDistance;
        fleeVector += transform.position;

        return fleeVector;
    }
}
