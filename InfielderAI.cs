using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using PathCreation;
using PathCreation.Examples;
using UnityEditor.Experimental.GraphView;

public class InfielderAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject baseball;
    public GameObject hand;

    private GameObject FirstBaseman;
    private GameObject SecondBaseman;
    private GameObject ThirdBaseman;
    private GameObject Shortstop;
    private GameObject LeftFielder;
    private GameObject RightFielder;
    private GameObject CenterFielder;
    private GameObject Pitcher;

    public GameObject FirstBase;
    public GameObject SecondBase;
    public GameObject ThirdBase;

    public NavHub navHub;
     public bool ballFielded = false;//
    //
    public Vector3 startingPos;
    TrajectoryPredictor tp;
    public int fielder;//
    public float throwAngle;
    public bool hasBall = false;
    public bool turnDoublePlay = false;
    [HideInInspector] public BaseManager baseOn = null;
    /**
     * Pitcher = 0, First = 1, Second = 2, Third = 3, Shortstop = 4
     * LeftField = 5, CenterField = 6, RightField = 7, Catcher = 8
     */
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        tp = baseball.GetComponent<TrajectoryPredictor>();
        startingPos = transform.position;
        FirstBaseman = navHub.FirstBase.gameObject;
        SecondBaseman = navHub.SecondBase.gameObject;
        ThirdBaseman = navHub.ThirdBase.gameObject;
        Shortstop = navHub.Shortstop.gameObject;
        LeftFielder = navHub.LeftField.gameObject;
        RightFielder = navHub.RightField.gameObject;
        CenterFielder = navHub.CenterField.gameObject;
        Pitcher = navHub.Pitcher.gameObject;

        

        
    }

    public void resetPlay()
    {
        hasBall = false;
        baseOn = null;
        turnDoublePlay = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        BallCollision bc = baseball.GetComponent<BallCollision>();
        if(other.gameObject == baseball && navHub.ballInPlay)
        {
            if (!navHub.ballThrown)
            {
                if (bc.touchedGround)
                {
                    navHub.ballFielded = true;
                    Rigidbody baseballRB = baseball.GetComponent<Rigidbody>();
                    baseballRB.velocity = new Vector3(0, 0, 0);
                    baseball.transform.position = hand.transform.position;
                    baseballRB.constraints = RigidbodyConstraints.FreezeAll;
                    baseball.GetComponent<SphereCollider>().enabled = false;
                    hasBall = true;
                    ballFieldedReceived();
                }
                else
                {
                    navHub.gameManager.PlayEvent_Out();
                    foreach(BaserunnerManager br in bc.brs)
                    {
                        Debug.Log("Checkpoint 3");
                        br.returnToBase();
                    }
                }

            }
            else
            {
                Rigidbody baseballRB = baseball.GetComponent<Rigidbody>();
                baseballRB.velocity = new Vector3(0, 0, 0);
                baseball.transform.position = hand.transform.position;
                baseballRB.constraints = RigidbodyConstraints.FreezeAll;
                baseball.GetComponent<SphereCollider>().enabled = false;

                hasBall = true;
                throwReceived();

                if (baseOn != null) { baseOn.inhabitantHasBall(); }

            }

        }
    }

    public void throwReceived()
    {
        if (fielder == 5 || fielder == 6 || fielder == 7)
        {
            navHub.outFieldThrowDecision(this);
        }
        else
        {
            navHub.infieldThrowDecisionReceived(this, turnDoublePlay);
        }
    }

    public void ballFieldedReceived()
    {
        if (navHub.ballInPlay)
        {
            if (fielder == 5 || fielder == 6 || fielder == 7)
            {
                navHub.outFieldThrowDecision(this);
            }
            else
            {
                navHub.infieldThrowDecisionFielded(this);
            }
        }
    }


    public float timeToPoint(Vector3 point)
    {
        Vector3 fielderPos = transform.position;
        fielderPos.y = 0;
        point.y = 0;
        float distance = Vector3.Distance(fielderPos, point);
        float time = distance / agent.speed;
        return time;
    }

    public void throwTo(Vector3 targetPos)
    {
        Vector3 throwDirection = targetPos - baseball.transform.position;
        float throwDistance = throwDirection.magnitude;

        float throwAngleRadians = throwAngle * Mathf.Deg2Rad;
        float gravity = Physics.gravity.magnitude; //should be -9.8?
        float velocity = Mathf.Sqrt((throwDistance * gravity) / Mathf.Sin(2 * throwAngleRadians));

        Rigidbody ballRB = baseball.GetComponent<Rigidbody>();
        Vector3 launchDirection = CalculateLaunchDirection(throwDirection, velocity, throwAngle);
        Debug.Log("throwTo");
        ballRB.constraints = RigidbodyConstraints.None;
        baseball.transform.parent = null;
        ballRB.velocity = launchDirection * velocity;
        hasBall = false;
        Invoke("ballThrown", 0.5f);
        
    }
    private void ballThrown()
    {
        navHub.ballThrown = true;
        baseball.GetComponent<SphereCollider>().enabled = true;
    }

    Vector3 CalculateLaunchDirection(Vector3 targetDirection, float velocity, float launchAngle)
    {
        Vector3 direction = targetDirection.normalized;

        float angleInRadians = launchAngle * Mathf.Deg2Rad;

        float horizontalSpeed = Mathf.Cos(angleInRadians) * velocity;
        float verticalSpeed = Mathf.Sin(angleInRadians) * velocity;

        Vector3 launchDirection = direction * horizontalSpeed;
        launchDirection.y = verticalSpeed;

        return launchDirection.normalized;
    }

    private void triggerBallThrown()
    {
        //baseball.GetComponent<SphereCollider>().enabled = true;
        //navHub.ballThrown = true;
    }

    private int throwRecursionCounter = 0;
    public void throwToFirst()
    {
        if (fielder == 1)
        {
            if(FirstBase.GetComponent<BaseManager>().inhabitant == Pitcher.GetComponent<InfielderAI>())
            {
                Vector3 targetPos = new Vector3(Pitcher.transform.position.x, Pitcher.transform.position.y + 1, Pitcher.transform.position.z);
                throwTo(targetPos);
            }
            else
            {
                agent.SetDestination(FirstBase.transform.position);
            }
        }
        else
        {
            if (FirstBase.GetComponent<BaseManager>().inhabitant == FirstBaseman.GetComponent<InfielderAI>())
            {
                //Debug.Log("inhabitant is first base. throwing now");
                Vector3 targetPos = new Vector3(FirstBaseman.transform.position.x, FirstBaseman.transform.position.y + 1, FirstBaseman.transform.position.z);
                throwTo(targetPos);
                throwRecursionCounter = 0;
            }
            else if (FirstBase.GetComponent<BaseManager>().inhabitant == Pitcher.GetComponent<InfielderAI>())
            {
                Vector3 targetPos = new Vector3(Pitcher.transform.position.x, Pitcher.transform.position.y + 1, Pitcher.transform.position.z);
                throwTo(targetPos);
                throwRecursionCounter = 0;
            }
            else if (navHub.ballInPlay && throwRecursionCounter < 50)
            {
                throwRecursionCounter++;
                Invoke("throwToFirst", 0.2f);
            }
        }

    }

    public void throwToSecond()
    {
        //Debug.Log("throw to second");
        if(SecondBase.GetComponent<BaseManager>().inhabitant == SecondBaseman.GetComponent<InfielderAI>())
        {
            Vector3 targetPos = new Vector3(SecondBaseman.transform.position.x, SecondBaseman.transform.position.y + 1, SecondBaseman.transform.position.z);
            throwTo(targetPos);
            throwRecursionCounter = 0;
        }
        else if(SecondBase.GetComponent<BaseManager>().inhabitant == Shortstop.GetComponent<InfielderAI>())
        {
            Vector3 targetPos = new Vector3(Shortstop.transform.position.x, Shortstop.transform.position.y + 1, Shortstop.transform.position.z);
            throwTo(targetPos);
            throwRecursionCounter = 0;
        }
        else if (navHub.ballInPlay && throwRecursionCounter < 50)
        {
            throwRecursionCounter++;
            Invoke("throwToSecond", 0.2f);
        }
    }

    public void throwToThird()
    {
        //Debug.Log("throw to third");
        if(ThirdBase.GetComponent<BaseManager>().inhabitant == ThirdBaseman.GetComponent<InfielderAI>())
        {
            Vector3 targetPos = new Vector3(ThirdBaseman.transform.position.x, ThirdBaseman.transform.position.y + 1, ThirdBaseman.transform.position.z);
            throwTo(targetPos);
            throwRecursionCounter = 0;
        }
        else if(navHub.ballInPlay && throwRecursionCounter < 50)
        {
            throwRecursionCounter++;
            Invoke("throwToThird", 0.2f);
        }
    }

    public void throwToHome()
    {
        Debug.Log("Throw to home");
    }

    public Vector3 intersectionPoint()
    {
        Vector3 fielderPos = transform.position;
        float fielderSpeed = agent.speed;
        Vector3 ballPos = baseball.transform.position;
        Vector3 ballVelocity = baseball.GetComponent<Rigidbody>().velocity;
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
        if(timeToIntercept < 0)
        {
            timeToIntercept = Mathf.Max(t1, t2);
        }
        if(timeToIntercept < 0)
        {
            return ballPos;
        }
        Vector3 intersectionPoint = ballPos + ballVelocity * timeToIntercept;
        return intersectionPoint;
    }

    public Vector3 futureIntersectionPoint(Vector3 futureBallPos)
    {
        Vector3 fielderPos = transform.position;
        float fielderSpeed = agent.speed;
        Vector3 ballPos = futureBallPos;
        Vector3 ballVelocity = baseball.GetComponent<Rigidbody>().velocity;
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

    //public Vector3 intersectionPoint()
    //{
    //    Vector3 fielderPos = transform.position;
    //    float fielderSpeed = agent.speed;
    //    Vector3 ballPos = baseball.transform.position;
    //    Vector3 ballVelocity = baseball.GetComponent<Rigidbody>().velocity;
    //    Vector3 relativePos = ballPos - fielderPos;
    //    float distanceToBall = relativePos.magnitude;
    //    float ballSpeed = ballVelocity.magnitude;
    //    float timeToIntercept = distanceToBall / ballSpeed;
    //    Vector3 interceptionPoint = ballPos + ballVelocity * timeToIntercept;
    //    return interceptionPoint;
    //}


    //public Vector3 intersectionPoint()
    //{
    //    Vector3 ballDirection = baseball.transform.position - transform.position;
    //    float timeToIntercept = ballDirection.magnitude / baseball.GetComponent<Rigidbody>().velocity.magnitude;
    //    Vector3 interceptionPoint = baseball.transform.position + baseball.GetComponent<Rigidbody>().velocity * timeToIntercept;
    //    return interceptionPoint;
    //}



    // Update is called once per frame
    void Update()
    {
        
    }

}
