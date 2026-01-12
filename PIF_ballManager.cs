using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PIF_ballManager : MonoBehaviour
{
    public GameObject playerFieldZone;
    public GameObject ground;
    public GameObject flyBallIndicator;
    private float[] verticies;
    private Vector3 ballStartingPos;
    private Rigidbody rb;
    public PIF_AnimationManager animationManager;
    public PIF_MovementManager movementManager;


    //public GameObject testIndicator;
    public float testVelocity;
    private float sendBouncerHeight;
    public bool sendBouncerActive = false;
    public bool sendGrounderActive = false;

    public bool flyBallInAir = false;
    private float flyBallTime = 0;
    private float flyBallCounter = 0;
    


    public bool testSendGrounder = false;
    public bool testSendBouncer = false;
    public bool testSendInfieldPopup = false;
    public bool testReset = false;


    private void Start()
    {
        ballStartingPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        if(collision.gameObject == ground)
        {
            if (sendBouncerActive) rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(2 * 9.81f * sendBouncerHeight), rb.velocity.z);
            //else if (sendGrounderActive) movementManager.grounderComing();
        }
    }
    private void Update()
    {
        if(testSendGrounder)
        {
            sendGrounder(testVelocity);
            testSendGrounder = false;
        }
        if(testSendBouncer)
        {
            sendBouncer(1f);
            testSendBouncer = false;
        }
        if(testSendInfieldPopup)
        {
            sendInfieldPopup();
            testSendInfieldPopup = false;
        }
        if (flyBallInAir)
        {
            flyBallCounter += Time.deltaTime;
            float timeLeft = flyBallTime - flyBallCounter;
            flyBallIndicator.transform.localScale = new Vector3(timeLeft * 3, 0.1f, timeLeft * 3);


            if (flyBallCounter > flyBallTime)
            {
                Debug.Log("end");
                flyBallInAir = false;
            }
        }
        if (testReset)
        {
            Time.timeScale = 1f;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.velocity = new Vector3(0, 0, 0);
            transform.parent = null;
            transform.position = ballStartingPos;
            testReset = false;
            sendBouncerActive = false;
            resetPlay();
        }
    }

    private void OnEnable()
    {
        verticies = getZoneVerticies();
    }

    public void resetPlay()
    {
        sendBouncerActive = false;
        sendGrounderActive = false;
        flyBallInAir = false;
        flyBallTime = 0;
        flyBallCounter = 0;
        movementManager.resetPlay();
        animationManager.resetPlay();
    }

    public void setPlayerFieldZoneSize(float x, float y, float z)
    {
        playerFieldZone.transform.localScale = new Vector3(x, y, z);
    }

    //Return x1, x2, z1, z2
    private float[] getZoneVerticies()
    {
        float radius = playerFieldZone.transform.localScale.x / 2;
        float x1 = playerFieldZone.transform.position.x + radius;
        float x2 = playerFieldZone.transform.position.x - radius;
        float z1 = playerFieldZone.transform.position.z + radius;
        float z2 = playerFieldZone.transform.position.z - radius;
        return new float[] { x1, x2, z1, z2 };
    }

    private bool pointWorks(Vector3 point)
    {
        if (point.x <= verticies[0] && point.x >= verticies[1] && point.z <= verticies[2] && point.z >= verticies[3])
        {
            return true;
        }
        else return false;
    }

    public Vector3 generateDestinationPoint()
    {
        float x = UnityEngine.Random.Range(verticies[1] * 10, verticies[0] * 10) / 10;
        float z = UnityEngine.Random.Range(verticies[3] * 10, verticies[2] * 10) / 10;
        float y = 0; //May change this in the future to detect the ground at that point
        Vector3 destinationPoint = new Vector3(x, y, z);
        if (pointWorks(destinationPoint)) return destinationPoint;
        else return generateDestinationPoint();

    }

    public void sendGrounder(float velocity)
    {
        sendGrounderActive = true;
        GetComponent<PathFollower>().enabled = false;
        Vector3 destinationPoint = generateDestinationPoint();
    //    testIndicator.transform.position = destinationPoint;

        Vector3 direction = (destinationPoint - transform.position).normalized;
        Vector3 targetVelocity = direction * velocity;

        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(Vector3.ClampMagnitude(targetVelocity, velocity), ForceMode.VelocityChange);

    }

    public void sendBouncer(float bounceHeight)
    {
        sendBouncerActive = true;
        Vector3 destinationPoint = generateDestinationPoint();
        destinationPoint.y = UnityEngine.Random.Range(150, 400) / 100;
        //testIndicator.transform.position = destinationPoint;

        sendBouncerHeight = destinationPoint.y;

        float horizontalDistanceRatio = 0.5f;
        float gravity = 9.81f;

        Vector3 horizontalDirection = new Vector3(destinationPoint.x - transform.position.x, 0, destinationPoint.z - transform.position.z);
        float horizontalDistance = horizontalDirection.magnitude;
        horizontalDirection.Normalize();

        float bouncePointDistance = horizontalDistance * horizontalDistanceRatio;
        Vector3 bouncePoint = transform.position + horizontalDirection * bouncePointDistance;

        float timeToBouncePeak = Mathf.Sqrt(2 * bounceHeight / gravity);
        float totalFlightTime = timeToBouncePeak * 2;

        float horizontalVelocity = bouncePointDistance / totalFlightTime;
        float verticalVelocity = Mathf.Sqrt(2 * gravity * bounceHeight);

        Vector3 launchVelocityVector = horizontalDirection * horizontalVelocity + Vector3.up * verticalVelocity;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = launchVelocityVector;
    }


    public void sendInfieldPopup()
    {
        Vector3 destinationPoint = generateDestinationPoint();
        flyBallIndicator.transform.position = destinationPoint;
        destinationPoint.y = 2f;
        //testIndicator.transform.position = destinationPoint;

        Vector3 launchDirection = destinationPoint - transform.position;
        float launchDistance = launchDirection.magnitude;
        float launchAngle = UnityEngine.Random.Range(450, 750) / 10;


        float launchAngleRadians = launchAngle * Mathf.Deg2Rad;
        float gravity = Physics.gravity.magnitude; //should be -9.8?
        float velocity = Mathf.Sqrt((launchDistance * gravity) / Mathf.Sin(2 * launchAngleRadians));

        Vector3 V3_launchDirection = CalculateLaunchDirection(launchDirection, velocity, launchAngle);
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity = V3_launchDirection * velocity;

        float timeToPeak = ((0 - rb.velocity.y) / (-9.8f));
        float peakY = (rb.velocity.y * timeToPeak) + (0.5f * -9.81f * (timeToPeak * timeToPeak));

        //
        flyBallInAir = true;
        flyBallTime = timeToPeak * 2;



        
        

    }


    private Vector3 CalculateLaunchDirection(Vector3 targetDirection, float velocity, float launchAngle)
    {
        Vector3 direction = targetDirection.normalized;

        float angleInRadians = launchAngle * Mathf.Deg2Rad;

        float horizontalSpeed = Mathf.Cos(angleInRadians) * velocity;
        float verticalSpeed = Mathf.Sin(angleInRadians) * velocity;

        Vector3 launchDirection = direction * horizontalSpeed;
        launchDirection.y = verticalSpeed;

        return launchDirection.normalized;
    }

   


}
