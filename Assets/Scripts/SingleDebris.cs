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

}
