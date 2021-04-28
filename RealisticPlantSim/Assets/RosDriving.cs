using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using geom_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;

public class RosDriving : MonoBehaviour
{
    private Vector3 targetPos;
    private bool move = false;

    private void Start()
    {
        //will only update the target position if we get a published message
        PositionSubscriber.Callback += updateTargetPos;
    }

    private void Update()
    {
        if (move)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 1f);
        }

        //stop if we get close enough to the target position
        if (Vector3.Distance(transform.position,targetPos) < 0.1f)
        {
            move = false;
            transform.position = targetPos;
        }
    }

    private void updateTargetPos(geom_msgs.Vector3 msg)
    {
        //im sanitizing the target position so we don't get any weird vertical movement
        move = true;
        targetPos = new Vector3((float)msg.x,transform.position.y,(float)msg.z);
    }
}
