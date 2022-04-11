using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;


public class MultipleDebris : Agent
{
    [SerializeField] private GameObject[] goals;
    private int counter = 0;
    private float moveSpeed = 3f;
    private float turnSpeed = 380f;
    private Vector3 startPosition;
    private Vector3 debrisPosition;
    new private Rigidbody rigidbody;


    public override void Initialize()
    // This gets called once at the beginning.
    {
        goals = new GameObject[transform.parent.Find("DebrisList").childCount];
        for (int i = 0; i < goals.Length; i++)
        {
            goals[i] = transform.parent.Find("DebrisList").GetChild(i).gameObject;
        }
        debrisPosition = transform.position; // Sneaky later things.
        startPosition = transform.position;
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    // This is called at the beginning of each episode. So when max step counter is achieved.
    {
        counter = 0;
        transform.rotation = Quaternion.identity;
        transform.position = startPosition; // Reset the agent.
        debrisPosition.y = 0.25f; // Reset the debris 

        for (int i = 0; i < goals.Length; i++)
        {
            goals[i].SetActive(true); // Reset the goal.
            goals[i].transform.position =
                debrisPosition
                + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f))
                * Vector3.forward * Random.Range(5f, 15f); // Reset the goal...but in a random location.
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int vertical = Mathf.RoundToInt(Input.GetAxis("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxis("Horizontal"));

        ActionSegment<int> actions = actionsOut.DiscreteActions;

        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // First check if the agent has strayed too far away from where it should be searching
        // If so, add a negative reward, and end the episode.
        if (Vector3.Distance(startPosition, transform.position) > 25f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1;

        if (horizontal != 0)
        {
            float angle = Mathf.Clamp(horizontal, -1f, 1f) * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up, angle);
        }

        Vector3 movement = transform.forward * Mathf.Clamp(vertical, -1f, 1f) * moveSpeed * Time.fixedDeltaTime;
        rigidbody.MovePosition(transform.position + movement);

        // TODO: Have the pill also tilt away...or some other way to make it visually obvious to an
        // outisde observer which way the agent is moving. Also it'd be kinda cute to see it zoomin'
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("goal"))
        {
            other.gameObject.SetActive(false);
            counter += 1;
            AddReward(1f);
            if (counter >= goals.Length)
            {
                EndEpisode();
            }
        }
    }

}
