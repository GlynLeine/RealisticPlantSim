using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient
{
    public class DepthBufferPublisher : UnityPublisher<std_msgs.UInt16MultiArray>
    {
        public string FrameId = "Test";

        [SerializeField]
        private Camera imageCamera;

        [SerializeField]
        private RenderTexture colorTexture;
        [SerializeField]
        private RenderTexture depthTexture;


        private Texture2D texture2D;
        private Rect rect;
        private int resolutionWidth = 1280;
        private int resolutionHeight = 720;
        private uint size;
        private std_msgs.UInt16MultiArray message;

        protected override void Start()
        {
            base.Start();
            PubCallback += sendDepthbuffer;
            InitializeGameObject();
            size = (uint)(resolutionHeight * resolutionWidth);
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB4444, false);
            depthTexture = new RenderTexture(resolutionWidth, resolutionHeight, 16, RenderTextureFormat.Depth);
        }

        private void UpdateDepthBuffer(ScriptableRenderContext context, Camera camera)
        {
            if (texture2D != null && imageCamera.gameObject.name.Equals(camera.gameObject.name))
            {
                colorTexture = imageCamera.targetTexture;
                imageCamera.targetTexture = depthTexture;
                rect = imageCamera.rect;
                imageCamera.targetTexture = colorTexture;
                texture2D.ReadPixels(rect, 0, 0);

                var multiArrayDims = new std_msgs.MultiArrayDimension[1] { new std_msgs.MultiArrayDimension("depthBuffer", size, sizeof(ushort)) };
                var multiArrayLayout = new std_msgs.MultiArrayLayout(multiArrayDims, 0);
                NativeArray<ushort> arr = texture2D.GetRawTextureData<ushort>();
                ushort[] data = arr.ToArray();
                var msg = new std_msgs.UInt16MultiArray(multiArrayLayout, data);
                //for(int i =0;i<10;i++)
                //{
                //    Debug.Log(data[i]);
                //}
                message = msg;
                PubCallback(msg);
            }
        }

        private void sendDepthbuffer(std_msgs.UInt16MultiArray msg)
        {
            Publish(msg);
        }

        void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += UpdateDepthBuffer;
        }
        void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= UpdateDepthBuffer;
        }
    }
}
