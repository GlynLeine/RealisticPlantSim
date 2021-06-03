using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class CameraPosePublisher : UnityPublisher<MessageTypes.Geometry.Pose>
    {
        public Transform PublishedTransform;

        private MessageTypes.Geometry.Pose message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.Pose();
            message.position = new MessageTypes.Geometry.Point();
            message.orientation = new MessageTypes.Geometry.Quaternion();
        }
        private void UpdateMessage()
        {
            Vector3 convertedPositon = PublishedTransform.localPosition.Unity2Ros();
            Quaternion convertedRotation = PublishedTransform.localRotation.Unity2Ros();

            message.position = new MessageTypes.Geometry.Point(convertedPositon.x,convertedPositon.y,convertedPositon.z);
            message.orientation = new MessageTypes.Geometry.Quaternion(convertedRotation.x,convertedRotation.y,convertedRotation.z,convertedRotation.w);

            Publish(message);
        }
    }
}
