using PathCreation;
using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NavHub : MonoBehaviour
{
    public GameManager gameManager;
    public MovementManager movementManager;
    public GameObject ball;
    public PathCreator pitchPath;
    public InfielderAI FirstBase;
    public InfielderAI SecondBase;
    public InfielderAI Shortstop;
    public InfielderAI ThirdBase;
    public InfielderAI LeftField;
    public InfielderAI RightField;
    public InfielderAI CenterField;
    public InfielderAI Pitcher;

    public GameObject _firstbase; //The actual base
    public GameObject _secondbase;
    public GameObject _thirdbase;
    
    [HideInInspector] public List<InfielderAI> Fielders = new List<InfielderAI>();
    [HideInInspector] public BaseManager[] baseManagers = new BaseManager[3];
    public TrajectoryPredictor tp;
    [HideInInspector] public bool ballFielded = false;
    [HideInInspector] public bool ballInPlay = false;
    public bool ballThrown = false;

    public void Start()
    {
        Fielders.Add(FirstBase);
        Fielders.Add(SecondBase);
        Fielders.Add(Shortstop);
        Fielders.Add(ThirdBase);
        Fielders.Add(LeftField);
        Fielders.Add(RightField);
        Fielders.Add(CenterField);
        Fielders.Add(Pitcher);
        baseManagers[0] = _firstbase.GetComponent<BaseManager>();
        baseManagers[1] = _secondbase.GetComponent<BaseManager>();
        baseManagers[2] = _thirdbase.GetComponent<BaseManager>();

    }

    public void playDead()
    {
        ballFielded = false;
        ballInPlay = false;
        BallCollision bc = ball.GetComponent<BallCollision>();
        bc.touchedGround = false;
        bc.ballMat.dynamicFriction = 0.3f;
        bc.ballMat.staticFriction = 0.3f;
        bc.batFollowHand.enabled = true;
        bc.bat.GetComponent<MeshCollider>().enabled = true;
        ball.GetComponent<SphereCollider>().enabled = true;
        ballThrown = false;
        foreach(InfielderAI fielder in Fielders)
        {
            fielder.resetPlay();
        }
        foreach(BaseManager bm in baseManagers)
        {
            bm.resetPlay();
        }

        //foreach(GameObject testIndicator in bc.testIndicators)
        //{
        //    Destroy(testIndicator);
        //}

        resetNavs();
    }


    public void resetNavs()
    {
       foreach(InfielderAI fielder in Fielders)
        {
            fielder.agent.SetDestination(fielder.startingPos);
        }
    }

    public InfielderAI baserunnerClosestToPopup;

    public void inTheAir()
    {
        InfielderAI closestDefender = closestToPopUp();
        baserunnerClosestToPopup = closestDefender;
        Vector3 predictionPoint = tp.predictionPoints[^1];
        closestDefender.agent.SetDestination(predictionPoint);
       // Debug.Log("inTheAir -- " + closestDefender + " sent to popup at point " + predictionPoint);

        if (closestDefender.timeToPoint(predictionPoint) > 0 && closestDefender.timeToPoint(predictionPoint) > ball.GetComponent<BallCollision>().timeToHitGround())
        {
           // Debug.Log(closestDefender + " will not make it. Initiating hitTheGround");
            hitTheGroundFuture(new Vector3(predictionPoint.x, 0, predictionPoint.z));
            
        }
    }

    public Vector3 baserunnerPopUpPoint;

    public InfielderAI closestToPopUp()
    {
        List<Vector3> points = tp.predictionPoints;
        Vector3 finalPoint = points[^1];
        baserunnerPopUpPoint = finalPoint;
        float minDistance = Vector3.Distance(FirstBase.transform.position, finalPoint);
        InfielderAI closest = FirstBase;
        foreach(InfielderAI fielder in Fielders)
        {
            float dis = Vector3.Distance(fielder.transform.position, finalPoint);
            if(dis < minDistance)
            {
                minDistance = dis;
                closest = fielder;
            }
        }
        return closest;
    }

    public InfielderAI baseRunnerInfieldBackup;

    public void hitTheGround()
    {
        //Debug.Log("hitTheGround");
        //Debug.Log("hitTheGround first choice");
        InfielderAI closest = sendClosestToIntersect(Fielders);
        InfielderAI infieldBackup = null;

        if (closest == Pitcher)
        {
            List<InfielderAI> newFielders = new(Fielders);
            newFielders.Remove(Pitcher);
           // Debug.Log("hitTheGround second choice");
            infieldBackup = sendClosestToIntersect(newFielders);
        }
        else infieldBackup = closest;
        baseRunnerInfieldBackup = infieldBackup;
        List<InfielderAI> outfielders = new List<InfielderAI> { LeftField, CenterField, RightField };
        //Debug.Log("hitTheGround third choice");
        InfielderAI thirdChoice = sendClosestToIntersect(outfielders);
        sendToBases(infieldBackup, closest);

    }

    public void sendToBases(InfielderAI ballFielder, InfielderAI potentialPitcher)
    {
        if (ballFielder.fielder == 1)
        {
            if(potentialPitcher.fielder == 0)
            {
                Vector3 ppIntersection = potentialPitcher.intersectionPoint();
                if(potentialPitcher.timeToPoint(ppIntersection) < ball.GetComponent<BallCollision>().timeToPoint(ppIntersection))
                {
                    FirstBase.agent.SetDestination(_firstbase.transform.position);
                }
                else
                {
                    Pitcher.agent.SetDestination(_firstbase.transform.position);
                }
            }
            SecondBase.agent.SetDestination(_secondbase.transform.position);
            ThirdBase.agent.SetDestination(_thirdbase.transform.position);
        }
        else if (ballFielder.fielder == 2)
        {
            FirstBase.agent.SetDestination(_firstbase.transform.position);
            Shortstop.agent.SetDestination(_secondbase.transform.position);
            ThirdBase.agent.SetDestination(_thirdbase.transform.position);
        }
        else if(ballFielder.fielder == 3)
        {
            FirstBase.agent.SetDestination(_firstbase.transform.position);
            SecondBase.agent.SetDestination(_secondbase.transform.position);

        }
        else
        {
            FirstBase.agent.SetDestination(_firstbase.transform.position);
            SecondBase.agent.SetDestination(_secondbase.transform.position);
            ThirdBase.agent.SetDestination(_thirdbase.transform.position);
        }

    }

        public InfielderAI whoWillFieldBall(InfielderAI firstChoice, InfielderAI secondChoice, InfielderAI thirdChoice)
    {
        Vector3 firstChoiceIntersect = firstChoice.intersectionPoint();
        Vector3 secondChoiceIntersect = secondChoice.intersectionPoint();
        BallCollision bc = ball.GetComponent<BallCollision>();
        if (firstChoice.timeToPoint(firstChoiceIntersect) < bc.timeToPoint(firstChoiceIntersect))
        {
            return firstChoice;
        }
        else if (secondChoice.timeToPoint(secondChoiceIntersect) < bc.timeToPoint(secondChoiceIntersect))
        {
            return secondChoice;
        }
        else return thirdChoice;
    }

    public void hitTheGroundFuture(Vector3 futureBallPos)
    {
        //Debug.Log("hitTheGround");
       // Debug.Log("hitTheGround first choice");
        InfielderAI closest = sendClosestToFutureIntersect(Fielders, futureBallPos);
        InfielderAI infieldBackup = null;

        if (closest == Pitcher)
        {
            List<InfielderAI> newFielders = new(Fielders);
            newFielders.Remove(Pitcher);
            //Debug.Log("hitTheGround second choice");
            infieldBackup = sendClosestToFutureIntersect(newFielders, futureBallPos);
        }
        else infieldBackup = closest;
        List<InfielderAI> outfielders = new List<InfielderAI> { LeftField, CenterField, RightField };
      //  Debug.Log("hitTheGround third choice");
        sendClosestToFutureIntersect(outfielders, futureBallPos);

    }

    public InfielderAI sendClosestToIntersect(List<InfielderAI> fielderList)
    {
        Vector3 closestIntersect = fielderList[0].intersectionPoint();
        float minDistance = Vector3.Distance(fielderList[0].transform.position, closestIntersect);
        InfielderAI closest = fielderList[0];
        foreach (InfielderAI fielder in fielderList)
        {
            Vector3 intersect = fielder.intersectionPoint();
            float dis = Vector3.Distance(fielder.transform.position, intersect);
            if (dis < minDistance)
            {
                closest = fielder;
                minDistance = dis;
                closestIntersect = intersect;
            }
        }
        closest.agent.SetDestination(closestIntersect);
        //Debug.Log("sendClosestToIntersect ----- " + closest + " sent to intersect at point " + closestIntersect);
        return closest;
    }

    public InfielderAI sendClosestToFutureIntersect(List<InfielderAI> fielderList, Vector3 futureBallPos)
    {
        Vector3 closestIntersect = fielderList[0].futureIntersectionPoint(futureBallPos);
        float minDistance = Vector3.Distance(fielderList[0].transform.position, closestIntersect);
        InfielderAI closest = fielderList[0];
        foreach (InfielderAI fielder in fielderList)
        {
            Vector3 intersect = fielder.futureIntersectionPoint(futureBallPos);
            float dis = Vector3.Distance(fielder.transform.position, intersect);
            if (dis < minDistance)
            {
                closest = fielder;
                minDistance = dis;
                closestIntersect = intersect;
            }
        }
        closest.agent.SetDestination(closestIntersect);
        //Debug.Log("sendClosestToIntersect ----- " + closest + " sent to intersect at point " + closestIntersect);
        return closest;
    }


    public InfielderAI fielderWithBall()
    {
        foreach(InfielderAI fielder in Fielders)
        {
            if (fielder.hasBall) return fielder;
        }
        return null;
    }

   

    public void outFieldThrowDecision(InfielderAI fielder)
    {
        if(movementManager.roundingThird)
        {
            fielder.throwToHome();
        }
        else if(movementManager.roundingSecond)
        {
            if (movementManager.halfWayToThird)
            {
                fielder.throwToHome();
            }
            else
            {
                fielder.throwToThird();
            }
        }
        else if(movementManager.roundingFirst)
        {
            fielder.throwToSecond();
        }
        else
        {
            fielder.throwToSecond();
        }
        movementManager.resetCheckpointIndicators();
    }

    public void infieldThrowDecisionFielded(InfielderAI fielder)
    {
        if(GodScript.manOnFirst)
        {
            fielder.throwToSecond();
            SecondBase.turnDoublePlay = true;
        }
        else
        {
            fielder.throwToFirst();
        }
        movementManager.resetCheckpointIndicators();
    }

    public void infieldThrowDecisionReceived(InfielderAI fielder, bool doublePlay)
    {
        if(doublePlay)
        {
            fielder.throwToFirst();
        }
        else if(movementManager.roundingFirst)
        {
            fielder.throwToSecond();
        }
        else if(movementManager.roundingSecond)
        {
            fielder.throwToThird();
        }
        else if(movementManager.roundingThird)
        {
            fielder.throwToHome();
        }
        else
        {
            Invoke("playDead", 0.5f);
        }
        movementManager.resetCheckpointIndicators();
    }





}
