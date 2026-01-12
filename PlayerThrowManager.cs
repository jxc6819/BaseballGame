using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerThrowManager : MonoBehaviour
{
    private TrajectoryPredictor tp;
    private Rigidbody rb;
    public GameObject firstBase;
    public GameObject secondBase;
    public GameObject thirdBase;
    public InputActionReference trigger;
    public InputActionReference primaryButton;
    public GameObject hand;
    public bool handInGlove = false;
    public bool primaryPressed = false;
    private PIF_ballManager ballManager;
    public XRInteractionManager interactionManager;
    List<Vector3> bases = new List<Vector3>();
    public PIF_AnimationManager animationManager;
    
    void Start()
    {
        tp = GetComponent<TrajectoryPredictor>();
        rb = GetComponent<Rigidbody>();
        ballManager = GetComponent<PIF_ballManager>();
        bases.Add(firstBase.transform.position);
        bases.Add(secondBase.transform.position);
        bases.Add(thirdBase.transform.position);
    }
    public void resetPlay()
    {
        handInGlove = false;
    }

    private void OnEnable()
    {
        trigger.action.started += TriggerEnabled;
        trigger.action.canceled += TriggerDisabled;
        primaryButton.action.started += PrimaryEnabled;
    }
    private void OnDisable()
    {
        trigger.action.started -= TriggerEnabled;
        trigger.action.canceled -= TriggerDisabled;
        primaryButton.action.started -= PrimaryEnabled;//
    }

   

    private void PrimaryEnabled(InputAction.CallbackContext context)
    {

        //if(!primaryPressed)
        //{

        //    animationManager.pitchBallPIF();
        //    primaryPressed = true;
        //}
        //else
        //{
        //    ballManager.testReset = true;
        //    animationManager.resetPlay();
        //    ballManager.resetPlay();
        //    primaryPressed = false;
        //}
    }

    private void TriggerEnabled(InputAction.CallbackContext context)
    {
        if(handInGlove)
        {
            // Debug.Log("handinglovetriggerpressed");
            transform.position = hand.transform.position;
            transform.parent = hand.transform;

        }
    }

    private void TriggerDisabled(InputAction.CallbackContext context)
    {
        if(transform.parent == hand.transform)
        {
            rb.constraints = RigidbodyConstraints.None;
            transform.parent = null;
            getPredictionPoints();
            Vector3 target = findTarget(bases);
            if (target.y == -100) rb.velocity = handVelocity;
            else 
            {
                target.y += 2;
                rb.velocity = ApplyAimAssist(target);
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            }
           
            //rb.angularVelocity = handAngularVelocity;

        }
    }


    void getPredictionPoints()
    {
       tp.Predict3D(rb);
    }


    public Vector3 ApplyAimAssist(Vector3 targetPos)
    {
        Vector3 directionToTarget = (targetPos-transform.position).normalized;
        Vector3 toTarget = (targetPos - transform.position);
        float disToTarget = toTarget.magnitude;
        //float timeToTarget = disToTarget / handVelocity.magnitude;   

        Vector3 initialHorizontalVelocity = new Vector3(handVelocity.x, 0, handVelocity.z);
        float horizontalSpeed = initialHorizontalVelocity.magnitude;
        Vector3 horizontalToTarget = new Vector3(toTarget.x, 0, toTarget.z);
        float horizontalDistance = horizontalToTarget.magnitude;
        float timeToTarget = horizontalDistance / horizontalSpeed;

        float gravity = Physics.gravity.y;
        float verticalDistance = targetPos.y - transform.position.y;
        float verticalVelocity = (verticalDistance - 0.5f * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        Vector3 horizontalDirection = horizontalToTarget.normalized;
        Vector3 adjustedVelocity = horizontalDirection * horizontalSpeed;
        adjustedVelocity.y = verticalVelocity;

        Vector3 finalVelocity = Vector3.Lerp(handVelocity, adjustedVelocity, 0.9f);
        return finalVelocity;


    }

    public Vector3 findTarget(List<Vector3> potentialTargets)
    {
        Vector3 closestTarget = potentialTargets[0];
        float smallestAngle = Mathf.Infinity;
        Vector3 throwDirection = handVelocity.normalized;

        foreach (Vector3 target in potentialTargets)
        {
            Vector3 toTarget = (target - transform.position).normalized;
            float angle = Vector3.Angle(throwDirection, toTarget);
            if (angle < smallestAngle)
            {
                smallestAngle = angle;
                closestTarget = target;
            }
        }

        return closestTarget;



        //List<Vector3> pp = tp.predictionPoints;
        //List<Vector3> choppingBlock = new List<Vector3>();
        //foreach(Vector3 target in targets)
        //{
        //    if (Vector3.Distance(new Vector3(target.x, 0, target.z), new Vector3(pp[0].x, 0, pp[0].z)) < Vector3.Distance(new Vector3(target.x, 0, target.y), new Vector3(pp[3].x, 0, pp[3].z)))
        //    {
        //        choppingBlock.Add(target);//
        //    }
        //}
        //foreach(Vector3 target in choppingBlock)
        //{
        //    if(targets.Contains(target)) targets.Remove(target);
        //}
        //Vector3 lastEligiblePP = pp[^1];
        //foreach(Vector3 point in pp)
        //{
        //    if (point.y > 1) lastEligiblePP = point;
        //}
        //if (targets.Count == 0) return new Vector3(0, -100, 0) ;

        //Vector3 bestTarget = targets[0];
        //float bestDis = Vector3.Distance(lastEligiblePP, targets[0]);

        //foreach(Vector3 target in targets)
        //{
        //    float dis = Vector3.Distance(lastEligiblePP, target);
        //    if(dis < bestDis)
        //    {
        //        bestTarget = target;
        //        bestDis = dis;
        //    }
        //}
        //return bestTarget;
    }

    public void ballThrown()
    {
        getPredictionPoints();
        
    }

    Vector3 oldHandPos = Vector3.zero;
    Vector3 newHandPos = Vector3.zero;
    Vector3 oldHandRot = Vector3.zero;
    Vector3 newHandRot = Vector3.zero;
    Vector3 handVelocity;
    Vector3 newHandVelocity;
    Vector3 oldHandVelocity;
    Vector3 newHandAngularVelocity;
    Vector3 oldHandAngularVelocity;
    Vector3 handAngularVelocity;
    private void Update()
    {
        oldHandRot = newHandRot;
        newHandRot = hand.transform.rotation.eulerAngles;
        oldHandPos = newHandPos;
        newHandPos = hand.transform.position;
        oldHandVelocity = newHandVelocity;
        newHandVelocity = (newHandPos - oldHandPos) / Time.deltaTime;
        oldHandAngularVelocity = newHandAngularVelocity;
        newHandAngularVelocity = (newHandRot - oldHandRot) / Time.deltaTime;

        handVelocity = Vector3.Lerp(oldHandVelocity, newHandVelocity, 0.5f);
        handAngularVelocity = Vector3.Lerp(oldHandAngularVelocity, newHandAngularVelocity, 0.5f);



        //handVelocity = (newHandPos - oldHandPos) / Time.deltaTime;

        //handAngularVelocity = (newHandRot - oldHandRot) / Time.deltaTime;
    }


}
