using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    public Transform target;
    public NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        agent.SetDestination(target.position);
    }
}