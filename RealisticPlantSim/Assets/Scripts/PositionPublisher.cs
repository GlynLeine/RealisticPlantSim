using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geom_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;

namespace RosSharp.RosBridgeClient
{
    public class PositionPublisher : UnityPublisher<geom_msgs.Vector3>
    {
        public string FrameId = "Test";

        protected override void Start()
        {
            base.Start();
            //PositionSubscriber.Callback += RespondNewPos;
        }

        private void RespondNewPos(geom_msgs.Vector3 message)
        {
            Debug.Log("Returning: "+ "{"+message.x+","+message.y+","+message.z+"}");
            Publish(message);
        }
    }
}
