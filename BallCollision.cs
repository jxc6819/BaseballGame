using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    PathFollower pathFollow;
    public GameObject bat;
    Rigidbody batRB;
    float pitchSpeed = 0;
    float qNum = 0.2f;
    Rigidbody rb;
    public NavHub navHub;
    public TrajectoryPredictor tp;
    public GameObject floor;
    private AudioSource batCrack;
    public PhysicMaterial ballMat;

    public GameObject testIndicator1;
    public GameObject testIndicator2;

    public BatFollowHand batFollowHand;
    public MovementManager movementManager;

    public bool touchedGround = false;
    //public BaserunnerManager br0;
    public BaserunnerManager br1;
    public BaserunnerManager br2;
    public BaserunnerManager br3;
    [HideInInspector] public BaserunnerManager[] brs;
    public GameManager gm;



    public void Start()
    {
        pathFollow = GetComponent<PathFollower>();
        batRB = bat.GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();
        batCrack = GetComponent<AudioSource>();
        pathFollow.enabled = false;
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
        transform.position = new Vector3(0, -1, 0);
        gameObject.SetActive(false);
        brs = new BaserunnerManager[] { br1, br2, br3 };
        
    }
    float calculateExitSpeed(float batSpeed)
    {
        pitchSpeed = pathFollow.speed;
        float pitchSpeedMPH = (qNum * (pitchSpeed*2.23f)) + ((1 + qNum) * (batSpeed * 1)); //2.23 used to be 1.5
        return pitchSpeedMPH / 1.5f;/// 2.237f;

    }

    public List<GameObject> testIndicators;
    private void testIndicatorTrajectory(List<Vector3> points, GameObject indicator)
    {
        foreach(Vector3 point in points)
        {
            GameObject indicatorObject = Instantiate(indicator, point, new Quaternion(0, 0, 0, 0));
            testIndicators.Add(indicatorObject);
        }
    }

    private void updateFriction()
    {
        while(navHub.ballInPlay)
        {
            if(Vector3.Distance(gameObject.transform.position, bat.transform.position) > Vector3.Distance(bat.transform.position, navHub._secondbase.transform.position))
            {
                ballMat.dynamicFriction = 0.6f;
                ballMat.dynamicFriction = 0.6f;

            }
        }
    }

    public float timeToHitGround()
    {
        float initialVelocity = GetComponent<Rigidbody>().velocity.y;
        float finalVelocity = -initialVelocity;
        if (initialVelocity < 0) return -1;
        return ((finalVelocity - initialVelocity) / (-9.8f));
    }

    public float timeToPoint(Vector3 targetPos)
    {
        Vector3 ballPos = transform.position;
        //ballPos.y = 0;
        //targetPos.y = 0;
        float distance = Vector3.Distance(ballPos, targetPos);
        float time = distance / GetComponent<Rigidbody>().velocity.magnitude;
        return time;
    }

    public float timeToPeak()
    {
        float initialVelocity = GetComponent<Rigidbody>().velocity.y;
        if (initialVelocity < 0) return -1;
        return ((0 - initialVelocity) / (-9.8f));
    }

    public void triggerRunning()
    {
        batFollowHand.enabled = false;
        movementManager.startRunning();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if (collision.gameObject == bat)
        {
            touchedGround = false;
            //bat.GetComponent<MeshCollider>().enabled = false;
            Debug.Log("hit the bat. \n Ball's point: " + gameObject.transform.position + "\nBat's point: " + bat.transform.position);
            navHub.ballInPlay = true;
            pathFollow.enabled = false;
            ///////
            float exitSpeedX = calculateExitSpeed(batRB.velocity.x);
            float exitSpeedY = calculateExitSpeed(batRB.velocity.y);
            float exitSpeedZ = calculateExitSpeed(batRB.velocity.z);
            if (batRB.velocity.x < 0) exitSpeedX *= -1;
            if(batRB.velocity.y < 0) exitSpeedY *= -1;
            if(batRB.velocity.z < 0) exitSpeedZ *= -1;
            Vector3 force = new Vector3(exitSpeedX, exitSpeedY, exitSpeedZ);
            rb.velocity = force;
            ///////
            tp.enabled = true;
            tp.Predict3D(rb);
           // testIndicatorTrajectory(tp.predictionPoints, testIndicator1);
           
            navHub.inTheAir();
            batCrack.Play();
            Debug.ClearDeveloperConsole();
            movementManager.enabled = true;
            Invoke("triggerRunning", 0.8f);
            // Invoke("reEnableBat", 1);
            gm.ballInPlay = true;

           foreach(BaserunnerManager br in brs)
            {
                br.Invoke("ballIsHit", 0.5f);
            }
        }
        if(collision.gameObject == floor)
        {
            //Debug.Log("Impact with the ground.");
            //tp.Predict3D(rb);
            //testIndicatorTrajectory(tp.predictionPoints, testIndicator2);
            touchedGround = true;
            navHub.hitTheGround();
            

        }
    }
}
