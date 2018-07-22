using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Witcher : MonoBehaviour {
    public Transform target;
    Animator animator;
    Vector3 destination;
    NavMeshAgent agent;

    void Start()
    {
        // Cache agent component and destination
        agent = GetComponent<NavMeshAgent>();
        destination = agent.destination;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("InputVertical", agent.speed * 0.2f);
        // Update destination if the target moves one unit
        if (Vector3.Distance(destination, target.position) > 1.0f)
        {
            destination = target.position;
            agent.destination = destination;
        }
    }
}
