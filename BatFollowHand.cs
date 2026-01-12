using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatFollowHand : MonoBehaviour
{
    public GameObject controller;
    public Rigidbody batRB;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        batRB.MovePosition(controller.transform.position);
        batRB.MoveRotation(controller.transform.rotation);

    }
}
