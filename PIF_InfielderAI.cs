using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PIF_InfielderAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject myBase;
    public int position; //0 = pitcher, 1 = first, 2 = second, 3 = third, 4 = shortstop, you know the rest
    public int playerPosition; //0 = pitcher, 123 = yeah, 4 = shortstop,  5 = leftfield, 6 = center 7 = right, 8 = catcher
    public GameObject ball;
    public bool hasBall;
    public Vector3 startingPos;
    [HideInInspector] public PIF_BaseManager baseOn;


    public void Start()
    {
        //if player is first, shortstop, pitcher, or right/center, second base covers
        //if player is third,catcher, second, or left, shortstop covers
        startingPos = transform.position;

        if (position == 4)
        {
            if (playerPosition == 1 || playerPosition == 0 || playerPosition == 7 || playerPosition == 6) myBase = null;
        }
        else if (position == 2)
        {
            if (playerPosition == 3 || playerPosition == 8 || playerPosition == 5) myBase = null;
        }
    }

    public void resetPlay()
    {
        transform.position = startingPos;
        hasBall = false;

    }
    public void ballIsHit()
    {
        if (position == 5)
        {
            if (playerPosition == 3 || playerPosition == 4) moveToBackup();
        }
        else if (position == 6)
        {
            if (playerPosition == 2 || playerPosition == 4) moveToBackup();
        }
        else if (position == 7)
        {
            if (playerPosition == 1 || playerPosition == 2) moveToBackup();
        }
        else if (myBase != null) agent.SetDestination(myBase.transform.position);
    }

    public void moveToBackup()
    {

    }
    
    public void caughtTheBall()
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ball)
        {
            caughtTheBall();
            hasBall = true;

            if (baseOn != null)
            {
                baseOn.inhabitantHasBall();
            }
        }
    }

}
