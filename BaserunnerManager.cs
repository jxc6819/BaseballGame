using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.AI;

public class BaserunnerManager : MonoBehaviour
{
    public int baseOn; //1, 2, 3
    public bool playerBatting;
    public NavHub navHub;
    public GameManager gm;
    public BallCollision ballCollision;
    public GameManager gameManager;
    public BaseManager firstBase;
    public BaseManager secondBase;
    public BaseManager thirdBase;
    public BaseManager homeBase;
    private BaseManager[] bases;
    public bool ballInOutField; //to do
    public PathCreator brPC;
    public Transform checkpoint1; //start
    public Transform checkpoint2; //halfway down first base line
    public Transform checkpoint3a; //rounding first
    public Transform checkpoint3b; //run through first
    public Transform checkpoint4; //first base
    public Transform checkpoint5; //second base
    public Transform checkpoint6; //round from second to third
    public Transform checkpoint7; //third
    public Transform checkpoint8; //rounding third
    public Transform checkpoint9; //home
    public Transform leadOff1;
    public Transform leadOff2;
    public Transform leadOff3;
    public GameObject baseball;
    
    


  
    private void Start()
    {
        bases = new BaseManager[] {firstBase, secondBase, thirdBase, homeBase };
       
        
    }

    public bool isDoneWithPlay()
    {
        if (!gameObject.activeInHierarchy || GetComponent <PathFollower>().enabled == false) return true;
        if(GetComponent<PathFollower>().distanceTravelled >= brPC.path.length)
        {
            return true;
        }
        return false;
    }
    

    



    private void OnEnable()
    {
        resetPlay();
        switch (baseOn)
        {
            case 1: transform.position = leadOff1.position; break;
            case 2: transform.position = leadOff2.position; break;
            case 3: transform.position = leadOff3.position; break;
            default: break;
        }
    }

    public void resetPlay()
    {
        GetComponent<PathFollower>().distanceTravelled = 0;
        GetComponent<PathFollower>().enabled = false;
        returningToBase = false;
        switch(baseOn)
        {
            case 1: transform.position = leadOff1.position; break;
            case 2: transform.position = leadOff2.position; break;
            case 3: transform.position = leadOff3.position; break;
            default: break;
        }
    }

    public void checkpointTriggered(int trig)
    {
        
            if (gm.ballInPlay)
            {
                switch (trig)
                {
                    case 2: checkpoint2Triggered(); break;
                    case 4: checkpoint4Triggered(); break;
                    case 5: checkPoint5Triggered(); break;
                    case 7: checkPoint7Triggered(); break;
                    case 9: checkPoint9Triggered(); break;
                    default: break;
                }
            }
        
    }

    public void checkpoint2Triggered()
    {
        if(shouldRunnerContinueRunning())
        {
            continueRunning(1);
        }
    }
    public void checkpoint4Triggered()
    {
        if(shouldRunnerContinueRunning())
        {
            continueRunning(2);
        }
    }
    public void checkPoint5Triggered()
    {
        reachedSecond();
    }
    public void checkPoint7Triggered()
    {
        reachedThird();
    }
    public void checkPoint9Triggered()
    {
        reachedHome();
    }

    public void ballIsHit()
    {
        Debug.Log("BRM" + baseOn + " ballIsHit");
        if(baseOn == 0)
        {
            BezierPath brBZ = new BezierPath(new Vector3[] { transform.position, checkpoint2.position }, false, PathSpace.xz);
            setUpPathCreator(brBZ);
            GetComponent<PathFollower>().enabled = true;
        }
        else if(baseOn == 1)
        {
            //advance base
            if (!willBallBeCaught()) advanceBase();
            else { Debug.Log("Checkpoint 1a"); returnToBase(); }
        }
        else if(baseOn == 2)
        {
            if(thirdBase.ForceOut)
            {
                //advance base
                if (!willBallBeCaught()) advanceBase();
                else { Debug.Log("Checkpoint 1b"); returnToBase(); }
            }
            else
            {
                bool advance = shouldRunnerAdvanceOffHit();
                if (advance)
                {
                    //advance base
                    advanceBase();
                }
                else
                {
                    //go back to base
                    Debug.Log("Checkpoint 2b");
                    returnToBase();
                }
            }
        }
        else if(baseOn == 3)
        {
            if(homeBase.ForceOut)
            {
                if (!willBallBeCaught()) advanceBase();
                else returnToBase();
            }
            else
            {
                bool advance = shouldRunnerAdvanceOffHit();
                if(advance)
                {
                    //advance base
                    advanceBase();
                }
                else
                {
                    //go back to base
                    advanceBase();
                }
            }
        }
    }

    private bool willBallBeCaught()
    {
        InfielderAI fielder = navHub.baserunnerClosestToPopup;
        //Debug.Log("Fielder: " + fielder.fielder);
        //Debug.Log("Time to point: " + fielder.timeToPoint(navHub.baserunnerPopUpPoint));
        //Debug.Log("Ball time to ground: " + ballCollision.timeToHitGround());
        if (fielder.timeToPoint(navHub.baserunnerPopUpPoint) < ballCollision.timeToHitGround())
        {
            return true;
        }
        else return false;
    }

    private void setUpPathCreator(BezierPath br)
    {
        
        br.ControlPointMode = BezierPath.ControlMode.Free;
        brPC.bezierPath = br;
        brPC.bezierPath.Space = PathSpace.xyz;
        brPC.bezierPath.GlobalNormalsAngle = 90f;
        br.ControlPointMode = BezierPath.ControlMode.Automatic;
    }
    public void advanceBase()
    {
        BezierPath brBZ = createBezierBaserunPathFromStart(baseOn+1);
        setUpPathCreator(brBZ);
        GetComponent<PathFollower>().enabled = true;
        GetComponent<Animator>().SetBool("isRunning", true);
    }

    public void continueRunning(int checkpoint)
    {
        // 1 = halfway to first
        // 2 = first
        // 3 = second
        // 4 = third
        if(checkpoint == 1)
        {
            brPC.bezierPath.AddSegmentToEnd(checkpoint3a.position);
            brPC.bezierPath.AddSegmentToEnd(checkpoint4.position);
        }
        else if(checkpoint == 2)
        {
            brPC.bezierPath.AddSegmentToEnd(checkpoint5.position);
        }
        else if(checkpoint == 3)
        {
            brPC.bezierPath.AddSegmentToEnd(checkpoint6.position);
            brPC.bezierPath.AddSegmentToEnd(checkpoint7.position);
        }
        else if(checkpoint == 4)
        {
            brPC.bezierPath.AddSegmentToEnd(checkpoint8.position);
            brPC.bezierPath.AddSegmentToEnd(checkpoint9.position);

        }
    }

    public BezierPath createBezierBaserunPathFromStart(int toBase)
    {
        Vector3[] pathPoints = new Vector3[] { };
        if(toBase == 2)
        {
            pathPoints = new Vector3[] { transform.position, checkpoint5.position };
        }
        else if(toBase == 3)
        {
            pathPoints = new Vector3[] { transform.position, checkpoint6.position, checkpoint7.position };   
        }
        else if(toBase == 4)
        {
            pathPoints = new Vector3[] { transform.position, checkpoint9.position };
        }
        BezierPath brBZ = new BezierPath(pathPoints, false, PathSpace.xz);
        return brBZ;
    }

    public bool returningToBase;
    public void returnToBase()
    {
        //Debug.Log("Baserunner: " + gameObject.name);
        //Debug.Log("BaseOn: " + baseOn);
        //Debug.Log("base go to: " + bases[baseOn - 1].gameObject.name);
        Debug.Log("return to base " + gameObject.name);
        
        BezierPath brBZ = new BezierPath(new Vector3[] { transform.position, bases[baseOn - 1].transform.position }, false, PathSpace.xz);
        setUpPathCreator(brBZ);
        GetComponent<Animator>().SetBool("isRunning", true);
        GetComponent<PathFollower>().distanceTravelled = 0;
        GetComponent<PathFollower>().enabled = true;
        returningToBase = true;
        
    }
    

    

    private bool infielderThreat(InfielderAI infielder)
    {
        if(baseOn == 2)
        {
            if (infielder.fielder == 4)
            {
                if (infielder.timeToPoint(infielder.intersectionPoint()) > ballCollision.timeToPoint(infielder.intersectionPoint()))
                {
                    return false;
                }
                return true;
            }
        }
        return false;
    }

    private bool shouldRunnerAdvanceOffHit()
    {
        if(playerBatting)
        {
            if (ballCollision.touchedGround)
            {
                if (GodScript.GameState == 0)
                {
                    InfielderAI infielder = navHub.baseRunnerInfieldBackup;
                    if (infielderThreat(infielder))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (baseOn == 2 && GodScript.playerFieldingPosition == 4) return false;
                    else return true;
                }
            }
            else
            {
                if (GodScript.GameState == 0)
                {
                    InfielderAI fielder = navHub.baserunnerClosestToPopup;
                    if (fielder.timeToPoint(navHub.baserunnerPopUpPoint) > ballCollision.timeToHitGround())
                    {
                        return true;
                    }
                    else return false;
                }
                else return false;
            }
        }
        else
        {
            if (ballCollision.touchedGround) return true;
            return false;
        }
    }

    private bool shouldRunnerContinueRunning()
    {
        Debug.Log("shouldRunnerContinueRunning");
        if (Vector3.Distance(baseball.transform.position, homeBase.transform.position) > Vector3.Distance(homeBase.transform.position, secondBase.transform.position) + (Vector3.Distance(homeBase.transform.position, secondBase.transform.position) / 2)  && !gm.ballFielded) return true;
        else return false;
    }

    

    private void reachedSecond()
    {
        Debug.Log("reached second");
        baseOn = 2;
        if (shouldRunnerContinueRunning())
        {
            Debug.Log("they should continue running");
            continueRunning(3);
        }
        else
        {
            Debug.Log("they shant continue running");
            GetComponent<Animator>().SetBool("isRunning", false);
        }
    }

    private void reachedThird()
    {
        baseOn = 3;
        if (shouldRunnerContinueRunning()) continueRunning(4);
        else
        {
            GetComponent<Animator>().SetBool("isRunning", false);
        }
    }

    private void reachedHome()
    {
        Debug.Log("Reached home");
        baseOn = 0;
        if (playerBatting) GodScript.playerScore++;
        else GodScript.cpuScore++;
        gameObject.SetActive(false);
    }
}
