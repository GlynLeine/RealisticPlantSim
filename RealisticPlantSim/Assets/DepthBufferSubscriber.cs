using UnityEngine;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(RosConnector))]
    public class DepthBufferSubscriber : UnitySubscriber<std_msgs.UInt8MultiArray>
    {
        public RawImage meshRenderer;
        private Texture2D texture2D;
        private byte[] depthBuffer;
        private Color[] colors;
        private bool isMessageReceived;

        protected override void Start()
        {
            base.Start();
            texture2D = new Texture2D(1280, 720);
            //meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }

        protected override void ReceiveMessage(std_msgs.UInt8MultiArray depthBuffer)
        {
            this.depthBuffer = depthBuffer.data;
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            colors = new Color[depthBuffer.Length * 4];
            for (int i = 0; i < depthBuffer.Length; i++)
            {
                float[] tempBuffer = new float[4];
                for (int j = 0;j<tempBuffer.Length;j++)
                {
                    tempBuffer[j] = depthBuffer[i] >> j * 8;
                }
                colors[i] = new Color(tempBuffer[0], tempBuffer[1], tempBuffer[2], tempBuffer[3]);
            }

            texture2D.SetPixelData<Color>(colors,0);
            texture2D.Apply();
            meshRenderer.texture = texture2D;
            meshRenderer.Rebuild(CanvasUpdate.MaxUpdateValue);
            isMessageReceived = false;
        }

    }
}
