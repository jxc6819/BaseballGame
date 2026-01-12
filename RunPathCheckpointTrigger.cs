using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunPathCheckpointTrigger : MonoBehaviour
{
    public int checkpoint;
    public GameObject player;
    public MovementManager movementManager;
    public BaserunnerManager brm0;
    public BaserunnerManager brm1;
    public BaserunnerManager brm2;
    public BaserunnerManager brm3;

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            movementManager.checkPointTriggered(checkpoint);
        }
        else if (other.gameObject == brm0.gameObject)
        {
            brm0.checkpointTriggered(checkpoint);
        }
        else if (other.gameObject == brm1.gameObject)
        {
            Debug.Log("checkpoint " + checkpoint + " triggered");
            brm1.checkpointTriggered(checkpoint);
        }
        else if (other.gameObject == brm2.gameObject)
        {
            brm2.checkpointTriggered(checkpoint);
        }
        else if(other.gameObject == brm3.gameObject)
        {
            brm3.checkpointTriggered(checkpoint);
        }

        if(checkpoint == 9)
        {
            Debug.Log("CHECKPOINT9 " + other.gameObject.name + " ---- " + gameObject.name);
        }
    }
}
