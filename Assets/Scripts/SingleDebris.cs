using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;


public class SingleDebris : Agent
{
    [SerializeField] private GameObject goal;

    private int counter = 1;
    private float moveSpeed = 3f;
    private float turnSpeed = 380f;
    private Vector3 startPosition;
    private Vector3 debrisPosition;
    new private Rigidbody rigidbody;


    public override void Initialize()
    // This gets called once at the beginning.
    {
        debrisPosition = transform.position; // Sneaky later things.
        startPosition = transform.position;
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    // This is called at the beginning of each episode. So when max step counter is achieved.
    {
        goal.SetActive(true); // Reset the goal.
        transform.position = startPosition; // Reset the agent.
        debrisPosition.y = 0.25f; // Reset the debris 
        goal.transform.position = debrisPosition + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * 5f; // Reset the goal...but in a random location.
    }

}
