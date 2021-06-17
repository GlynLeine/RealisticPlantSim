/// <summary>
/// Publishes position requests to ROS
/// Author: Rowan Ramsey
/// </summary>
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
            sendPosition(new geom_msgs.Vector3());
            PubCallback += sendPosition;
        }

        private void sendPosition(geom_msgs.Vector3 msg)
        {
            Publish(msg);
        }
    }
}
