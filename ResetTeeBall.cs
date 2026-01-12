using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTeeBall : MonoBehaviour
{
    public GameObject ball;
    private Vector3 ballStartingPos;
    PathFollower pathFollow;
    TrajectoryPredictor tp;
    public NavHub nh;
    public GameManager gameManager;

    public void Start()
    {
        ballStartingPos = ball.transform.position;
        pathFollow = ball.GetComponent<PathFollower>();
        tp = ball.GetComponent <TrajectoryPredictor>();
        

    }
    private void OnTriggerEnter(Collider other)
    {
        tp.enabled = false;
        //ball.transform.position = ballStartingPos;
        ////Rigidbody ballRigidBody = ball.GetComponent<Rigidbody>();
        //ballRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        ////ballRigidBody.velocity = new Vector3(0f,0f,0f);
        gameManager.playDead();
        ball.transform.position = new Vector3(0, -1, 0);
        nh.resetNavs();
        Invoke("pitchBall", 5);
    }

    public void pitchBall()
    {
        ball.SetActive(true);
        ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        pathFollow.enabled = true;
        pathFollow.distanceTravelled = 0;
    }
}
