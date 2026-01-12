using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
public class GloveManager : MonoBehaviour
{
    public GameObject ball;
    public Transform ballCaughtPos;
    public GameObject hand;
    public PlayerThrowManager ptm;
    private Rigidbody ballRB;
    public bool hasBall = false;
    public PIF_GameManager gm;
    [SerializeField] private XRBaseController controller;
    

    private void Start()
    {
        ballRB = ball.GetComponent<Rigidbody>();
    }

    public void resetPlay()
    {
        hasBall = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == ball)
        {
            ballCaught();
        }
        if(other.gameObject == hand)
        {
            ptm.handInGlove = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject == hand)
        {
            ptm.handInGlove = false;
        }
    }

    public void ballCaught()
    {
        Debug.Log("ball caught");
        gm.ballFielded = true;
        controller.SendHapticImpulse(1f, 0.5f);
        hasBall = true;
        ballRB.velocity = new Vector3(0, 0, 0);
        ballRB.constraints = RigidbodyConstraints.FreezeAll;
        ball.transform.position = ballCaughtPos.position;
        ball.transform.parent = ballCaughtPos;
    }
}
