using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using UnityEngine.InputSystem;
using PathCreation.Examples;
using System.Diagnostics.Contracts;

public class MovementManager : MonoBehaviour
{
    public GameManager gameManager;
    public NavHub navHub;
    public GameObject rightController;
    public GameObject leftController;
    public InputActionReference rightTrigger;
    public InputActionReference leftTrigger;
    public GameObject head;
    public GameObject player;
    public PathCreator baseRunPath;
    float normalHeight;
    public bool running;
    Vector3 rcPos;
    Vector3 lcPos;

    //Baserun checkpoints
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
    public Vector3[] baseRunPathPoints;
    private BezierPath baseRunBezier;
    [SerializeField] private bool rightTriggerDown;
    [SerializeField] private bool leftTriggerDown;
    [SerializeField] private bool bothTriggersDown;
    public PathFollower pathFollower;

    private void Start()
    {
        EnableTriggerWatching();
        rcPos = rightController.transform.position;
        lcPos = leftController.transform.position;
        normalHeight = head.transform.position.y;
        

        //baseRunPathPoints = new Vector3[] {checkpoint1.position, checkpoint2.position, checkpoint3a.position, checkpoint4.position,
        //                                   checkpoint5.position, checkpoint6.position, checkpoint7.position, checkpoint8.position,
        //                                   checkpoint9.position};

        //baseRunPathPoints = new Vector3[] { checkpoint1.position, checkpoint2.position, checkpoint3a.position, checkpoint4.position };
        //baseRunBezier = new BezierPath(baseRunPathPoints, false, PathSpace.xz);
        
        //baseRunBezier.ControlPointMode = BezierPath.ControlMode.Automatic;
        //baseRunPath.bezierPath = baseRunBezier;
        
        

    }

    public void resetPlay()
    {
        stoppingAtFirst = false;
        pathFollower.distanceTravelled = 0;
        pathFollower.enabled = false;
        stoppedRunning = false;
        resetCheckpointIndicators();
    }

    public void resetCheckpointIndicators()
    {
        roundingFirst = false;
        roundingSecond = false;
        halfWayToThird = false;
        roundingThird = false;
    }




    public void startRunning()
    {
        Vector3 startPoint = new Vector3(head.transform.position.x, 0, head.transform.position.z);
        baseRunPathPoints = new Vector3[] { startPoint, checkpoint1.position, checkpoint2.position};
        baseRunBezier = new BezierPath(baseRunPathPoints, false, PathSpace.xz);
        baseRunBezier.ControlPointMode = BezierPath.ControlMode.Automatic;
        baseRunPath.bezierPath = baseRunBezier;
        baseRunPath.bezierPath.Space = PathSpace.xyz;
        baseRunPath.bezierPath.GlobalNormalsAngle = 90;
        pathFollower.enabled = true;
        gameManager.bat.SetActive(false);

    }
    private int stopSwingingCounter = 0;
    void Update()
    {
        if (Vector3.Distance(rightController.transform.position, rcPos) > 0.25f || Vector3.Distance(leftController.transform.position, lcPos) > 0.25f)
        {
            running = true;
            //Debug.Log("running");
            pathFollower.speed = 6;
            stopSwingingCounter = 0;
        }
        else
        {
            stopSwingingCounter++;
            running = false;
        }

        if(stopSwingingCounter > 10) pathFollower.speed = 4;
        rcPos = rightController.transform.position;
        lcPos = leftController.transform.position;
        //if(testBool)
        //{
        //    baseRunBezier.AddSegmentToEnd(checkpoint5.position);
        //}
        Vector3[] test = baseRunPath.path.localPoints;
       
    }

    public void EnableTriggerWatching()
    {
        leftTrigger.action.started += leftTriggerActivated;
        leftTrigger.action.canceled += leftTriggerDeactivated;
        rightTrigger.action.started += rightTriggerActivated;
        rightTrigger.action.canceled += rightTriggerDeactivated;
    }

    public void DisableTriggerWatching()
    {
        leftTrigger.action.started -= leftTriggerActivated;
        leftTrigger.action.canceled -= leftTriggerDeactivated;
        rightTrigger.action.started -= rightTriggerActivated;
        rightTrigger.action.canceled -= rightTriggerDeactivated;
    }

    private void leftTriggerActivated(InputAction.CallbackContext context)
    {
        leftTriggerDown = true;
        checkBothTriggers();
    }
    private void rightTriggerActivated(InputAction.CallbackContext context)
    {
        rightTriggerDown = true;
        checkBothTriggers();
    }
    private void leftTriggerDeactivated(InputAction.CallbackContext context)
    {
        leftTriggerDown = false;
        checkBothTriggers();
    }
    private void rightTriggerDeactivated(InputAction.CallbackContext context)
    {
        rightTriggerDown = false;
        checkBothTriggers();
    }
    private void checkBothTriggers()
    {
        if (rightTriggerDown && leftTriggerDown) bothTriggersDown = true;
        else bothTriggersDown = false;
    }


    public void checkPointTriggered(int checkpoint)
    {
        switch(checkpoint)
        {
            case 2: checkPoint2Triggered(); break;
            case 3: checkPoint3aTriggered(); break;
            case -3: checkPoint3bTriggered(); break;
            case 4: checkPoint4Triggered(); break;
            case 5: checkPoint5Triggered(); break;
            case 6: checkPoint6Triggered(); break;
            case 7: checkPoint7Triggered(); break;
            case 8: checkPoint8Triggered(); break;
            case 9: checkPoint9Triggered(); break;
            default: Debug.Log("switch statement default"); break;

        }
    }

    private bool stoppingAtFirst = false;
    private void checkPoint2Triggered()
    {
        if (bothTriggersDown)
        {
            baseRunBezier.AddSegmentToEnd(checkpoint3a.position);
        }
        else
        {
            stoppingAtFirst = true;
            baseRunBezier.AddSegmentToEnd(checkpoint4.position);
            baseRunBezier.AddSegmentToEnd(checkpoint3b.position);
        }
    }
    private void checkPoint3aTriggered()
    {
        baseRunBezier.AddSegmentToEnd(checkpoint4.position);
    }
    private void checkPoint3bTriggered()
    {
        stoppedRunning = true;
        //play is over.
       // Invoke("testingResetPlay", 2f);
    }

    public bool roundingFirst = false;
    public bool stoppedRunning = false;
    private void checkPoint4Triggered()
    {
        if(bothTriggersDown)
        {
            roundingFirst = true;
            baseRunBezier.AddSegmentToEnd(checkpoint5.position);
        }
        else
        {
            //play is over.
            stoppedRunning = true;
           // Invoke("testingResetPlay", 2f);
        }
    }
    public bool roundingSecond = false;
    private void checkPoint5Triggered()
    {
        if(bothTriggersDown)
        {
            roundingSecond = true;
            baseRunBezier.AddSegmentToEnd(checkpoint6.position);
        }
        else
        {
            stoppedRunning = true;
            //Invoke("testingResetPlay", 2f);
        }
    }

    public bool halfWayToThird = false;
    private void checkPoint6Triggered()
    {
        halfWayToThird = true;
        baseRunBezier.AddSegmentToEnd(checkpoint7.position);
    }

    public bool roundingThird = false;
    private void checkPoint7Triggered()
    {
        if(bothTriggersDown)
        {
            roundingThird = true;
            baseRunBezier.AddSegmentToEnd(checkpoint8.position);
        }
        else
        {
            stoppedRunning = true;
            //play is over.
           // Invoke("testingResetPlay", 2f);
        }
    }
    private void checkPoint8Triggered()
    {
        baseRunBezier.AddSegmentToEnd(checkpoint9.position);
    }
    private void checkPoint9Triggered()
    {
        //score!
        Invoke("testingResetPlay", 2f);
    }

    private void testingResetPlay()
    {
        gameManager.playDead();
    }
}
