/// <summary>
/// Handles the movement and rotation of the robot
/// Author: Rowan Ramsey
/// </summary>
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
    private bool useAi = false;

    [SerializeField]
    private float moveStep = 1;
    [SerializeField]
    private float rotationStep = 1;
    [SerializeField]
    private bool rotateFirst = false;

    private void Start()
    { 
        //Whenever the SubCallback function is called we want to update the target position
        PositionSubscriber.SubCallback += updateTargetPos;
    }

    private void OnDisable()
    {
        PositionSubscriber.SubCallback -= updateTargetPos;
    }

    private void Update()
    {
        //Moves the gameobject to the tartget position
        MoveToPoint();
    }

    private void updateTargetPos(geom_msgs.Vector3 msg)
    {
        Debug.Log(string.Format("Updated Pos: {0}", targetPos));

        targetPos = new Vector3((float)msg.x, transform.position.y, (float)msg.z);
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


    #region Move and Rotatation
    void MoveToPoint()
    {
        //Rotate towards the target before moving towards the target position
        if (rotate)
        {
            RotateToTarget();
        }

        //Move towards the target
        if (move)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);
        }

        //Stop if we get close enough to the target position
        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            move = false;
            //Set the robots position to the target position
            transform.position = targetPos;

            //Callback to return the current position back to ROS
            PositionPublisher.PubCallback(new geom_msgs.Vector3(transform.position.x, transform.position.y, transform.position.z));
        }
    }

    //Rotates the gameobject to point towards the target position
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
    #endregion
}
