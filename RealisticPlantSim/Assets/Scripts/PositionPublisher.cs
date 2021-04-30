using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using geom_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;

namespace RosSharp.RosBridgeClient
{
    public class PositionPublisher : UnityPublisher<geom_msgs.Vector3>
    {
        public delegate void PublisherCallback();
        public static PublisherCallback PubCallback;

        public string FrameId = "Test";

        protected override void Start()
        {
            base.Start();
            returnPosition() ;
            PubCallback += returnPosition;
        }

        private void returnPosition()
        {
            //Debug.Log("Returning: "+ "{"+message.x+","+message.y+","+message.z+"}");
            Publish(new geom_msgs.Vector3());
        }
    }
}
