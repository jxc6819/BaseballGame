using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    public int baseNum;
    private List<InfielderAI> Fielders;
    public InfielderAI inhabitant;
    public GameObject player;
    public NavHub navHub;
    public GameManager gameManager;

    [HideInInspector] public int inhabitantNum = -1;
    [HideInInspector] public bool playerOccupied = false;
    [HideInInspector] public bool baseRunnerOccupied = false;
    public bool ForceOut;

    public bool Safe = false;
    public bool Out = false;

    public void Start()
    {
        Fielders = navHub.Fielders;
    }

    public void resetPlay()
    {
        inhabitantNum = -1;
        inhabitant = null;
        playerOccupied = false;
        Safe = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<InfielderAI>() != null)
        {
            inhabitant = other.gameObject.GetComponent<InfielderAI>();
            inhabitantNum = inhabitant.fielder;
            inhabitant.baseOn = this;
            inhabitantOnBase();
        }
        if(other.gameObject == player)
        {
            playerOccupied = true;
            playerOnBase();
        }
        if(other.gameObject.GetComponent<BaserunnerManager>() != null)
        {
            baseRunnerOccupied = true;
            baseRunnerOnBase(other.GetComponent<BaserunnerManager>());
            
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponent<InfielderAI>() != null)
        {
            if(other.gameObject.GetComponent<InfielderAI>() == inhabitant)
            {
                inhabitant = null;
                inhabitantNum = -1;
                inhabitant.baseOn = null;
            }
        }
        if(other.gameObject == player)
        {
            playerOccupied = false;
            if (!gameManager.movementManager.stoppedRunning) Safe = false;
            
        }
        if(other.gameObject.GetComponent<BaserunnerManager>() != null)
        {
            baseRunnerOccupied = false;
            Safe = false;
        }
    }

   
    public void inhabitantHasBall()
    {
        //Debug.Log("inhabitantHasBall Triggered");
        if(!Safe && ForceOut)
        {
           // Debug.Log("inhabitantHasBall checkpoint 1");
            gameManager.PlayEvent_Out();
            Out = true;
        }
        else if(!Safe)
        {
            //tag player
        }
    }

    public void playerOnBase()
    {
        // Debug.Log("playerOnBase Triggered");
        if (inhabitant != null)
        {
            if (!inhabitant.hasBall && !baseRunnerOccupied && !Out)
            {
                //gameManager.PlayEvent_Safe(this);
                Safe = true;
                if (gameManager.movementManager.stoppedRunning)
                {
                    gameManager.PlayEvent_Safe(this);
                }
            }
        }
        else if(!Out)
        {
            Safe = true;
            if (gameManager.movementManager.stoppedRunning)
            {
                gameManager.PlayEvent_Safe(this);
            }
        }
    }

    public void baseRunnerOnBase(BaserunnerManager brm)
    {
        if (brm.returningToBase) brm.GetComponent<PathFollower>().enabled = false;
        if (inhabitant != null)
        {
            if (!inhabitant.hasBall)
            {
                Safe = true;
            }
        }
        else Safe = true;
    }
    public void inhabitantOnBase()
    {
        //Debug.Log("inhabitantOnBase Triggered");
        if (inhabitant.hasBall && !playerOccupied && !baseRunnerOccupied && ForceOut)
        {
            gameManager.PlayEvent_Out();
            Out = true;
        }
        else if(inhabitant.hasBall && !playerOccupied)
        {
            //tag player
        }
    }



}
