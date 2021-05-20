using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using geom_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;

public class RosDriving : MonoBehaviour
{

    private Vector3 targetPos;
    private Vector3 targetDirection;
    private Quaternion targetRotation;

    private bool move = false;
    private bool rotate = false;

    [SerializeField]
    private float moveStep = 1;
    [SerializeField]
    private float rotationStep = 1;
    [SerializeField]
    private bool rotateFirst = false;

    private void Start()
    {
        //will only update the target position if we get a published message
        PositionSubscriber.SubCallback += updateTargetPos;
    }

    private void Update()
    {
        //just moves the robot to the position, nothing fancy
        MoveToPoint();
    }

    private void updateTargetPos(geom_msgs.Vector3 msg)
    {
        Debug.Log("Updated Pos");
        targetPos = new Vector3((float)msg.x, transform.position.y, (float)msg.z);
        //im sanitizing the target position so we don't get any weird vertical movement
        if (rotateFirst)
        {
            rotate = true;
            move = false;
            targetDirection = (targetPos - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(targetDirection);
        }
        else
        {
            move = true;
        }

    }

    void MoveToPoint()
    {

        //rotate towards the target before we start moving
        if (rotate)
        {
            RotateToTarget();
        }
        //Debug.Log("Target Direction: "+targetDirection);

        //move towards the target
        if (move)
        {
            //Debug.Log("I am being called");
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);//moves at movestep unit intervals
        }

        //stop if we get close enough to the target position
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            move = false;
            //set the robots position to targetPos so we don't get any weird drift
            transform.position = targetPos;

            //this is purely a callback to request a new position
            PositionPublisher.PubCallback(new geom_msgs.Vector3(transform.position.x, transform.position.y, transform.position.z));
        }
    }

    void RotateToTarget()
    {
        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationStep);
            return;
        }
        transform.rotation = targetRotation;
        rotate = false;
        move = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (targetDirection * 10));
        //Gizmos.color = Color.blue;
        //Gizmos.DrawRay(transform.position,transform.forward);
    }
}
