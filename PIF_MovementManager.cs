using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class PIF_MovementManager : MonoBehaviour
{
    public GameObject rightController;
    public GameObject leftController;
    public GameObject player;
    public GameObject floor;
    public PathFollower pathFollow;
    public PathCreator path;
    private Vector3 rcPos;
    private Vector3 lcPos;
    private int stopSwingingCounter;
    public float speed;
    private Vector3 lastValidDirection;
    public GameObject ball;
    private Vector3 intersect;
    private Vector3 initialPlayerPos;
    private bool ballHit;
    private bool lockedOn;

   
    
    

    public void resetPlay()
    {
        pathFollow.enabled = false;
        ballHit = false;
        lockedOn = false;
        player.transform.position = initialPlayerPos;
    }
    private void Start()
    {
        lastValidDirection = rightController.transform.forward;
        lastValidDirection.y = 0;
        lastValidDirection.Normalize();
    }

    
    private void Update()
    {

        

        if (Vector3.Distance(rightController.transform.position, rcPos) > 0.05f || Vector3.Distance(leftController.transform.position, lcPos) > 0.05f)
        { 
            
            stopSwingingCounter = 0;
        }
        else
        {
            stopSwingingCounter++;
        }

        

        if (stopSwingingCounter < 15 && ballHit)
        {
            Vector3 direction = rightController.transform.forward;
            float angle = Vector3.Angle(direction, Vector3.up);

            if (angle < 130) //dont move it in that direction if the controller is facing down
            {
                direction.y = 0;
                direction.Normalize();
                lastValidDirection = direction;
            }
            player.transform.position += lastValidDirection * speed * Time.deltaTime;
        }
        rcPos = rightController.transform.position;
        lcPos = leftController.transform.position;
        //
        //Once the player has travelled halfway to where the intersect point is, they will automatically lock on to a path to the intersect
        if(ballHit && !lockedOn)
        {
            intersect = grounderIntersectPoint();
           
            if (Vector3.Distance(player.transform.position, intersect) < (Vector3.Distance(initialPlayerPos, intersect) / 2))
            {
                lockedOn = true;
                lockOnToIntersect();
                
            }
        }
    }

    public void ballIsHit()
    {
        
        ballHit = true;
        initialPlayerPos = player.transform.position;
        intersect = grounderIntersectPoint();

    }
    public void lockOnToIntersect()
    {
        intersect = grounderIntersectPoint();
        Vector3[] pathPoints = new Vector3[] { player.transform.position, intersect };
        BezierPath bz = new BezierPath(pathPoints, false, PathSpace.xz);
        bz.ControlPointMode = BezierPath.ControlMode.Automatic;
        path.bezierPath = bz;
        path.bezierPath.Space = PathSpace.xyz;
        path.bezierPath.GlobalNormalsAngle = 90f;
        path.bezierPath.ControlPointMode = BezierPath.ControlMode.Free;
        path.bezierPath.ControlPointMode = BezierPath.ControlMode.Automatic;
        pathFollow.enabled = true;
        pathFollow.distanceTravelled = 0;

    }


    private Vector3 grounderIntersectPoint()
    {
        //float friction = 0.3f;
        //float gravity = 9.81f;
        //float a = friction * gravity;

        //Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
        //float difMagnitude = ballVelocity.magnitude - 10f; //negative?
        //float t = (2 * difMagnitude) / a;

        //Vector3 distanceTravelled = ballVelocity * t - 0.5f * a * t * t * ballVelocity.normalized;

        //Vector3 intersectionPoint = ball.transform.position + distanceTravelled;
        Vector3 fielderPos = player.transform.position;
        float fielderSpeed = speed;
        Vector3 ballPos = ball.transform.position;
        Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;
        Vector3 relativePos = ballPos - fielderPos;

        float a = Vector3.Dot(ballVelocity, ballVelocity) - fielderSpeed * fielderSpeed;
        float b = 2.0f * Vector3.Dot(ballVelocity, relativePos);
        float c = Vector3.Dot(relativePos, relativePos);
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0) return ballPos;

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);

        float timeToIntercept = Mathf.Min(t1, t2);
        if (timeToIntercept < 0)
        {
            timeToIntercept = Mathf.Max(t1, t2);
        }
        if (timeToIntercept < 0)
        {
            return ballPos;
        }
        Vector3 intersectionPoint = ballPos + ballVelocity * timeToIntercept;
        return intersectionPoint;

    }



}
