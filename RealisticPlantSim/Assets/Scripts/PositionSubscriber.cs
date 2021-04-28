
using UnityEngine;
using geom_msgs = RosSharp.RosBridgeClient.MessageTypes.Geometry;
namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class PositionSubscriber : UnitySubscriber<geom_msgs.Vector3>
    {
        public delegate void SubscriberCallback(geom_msgs.Vector3 msg);
        public static SubscriberCallback Callback;

        private Vector3 position;
        private bool isMessageReceived = true;

        protected override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        private void ProcessMessage()
        {
            //this callback is used to send the position data to anything that wants to use it
            Callback(new geom_msgs.Vector3(position.x, position.y, position.z));
            //Debug.Log("Recieved: " + "{" + position.x + "," + position.y + "," +position.z + "}");
            isMessageReceived = false;
        }

        protected override void ReceiveMessage(geom_msgs.Vector3 message)
        {
            position = new Vector3((float)message.x,(float)message.y,(float)message.z);
            isMessageReceived = true;
        }

        public Vector3 GetPosition()
        {
            return position;
        }
    }
}
