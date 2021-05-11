using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;

namespace RosSharp.RosBridgeClient
{
    public class DepthBufferPublisher : UnityPublisher<std_msgs.UInt8MultiArray>
    {
        public string FrameId = "Test";

        [SerializeField]
        private Camera imageCamera;
        [SerializeField]
        private RenderTexture depthTexture;

        private int resolutionWidth = 1280;
        private int resolutionHeight = 720;
        private uint size;
        private std_msgs.UInt8MultiArray message;
        private bool sendMessage = true;
        private bool update = false;

        private Texture2D texture2D;
        private Rect rect;


        List<AsyncGPUReadbackRequest> requests =new List<AsyncGPUReadbackRequest>();


        protected override void Start()
        {
            base.Start();
            PubCallback += sendDepthbuffer;

            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB4444, false);
            size = (uint)(resolutionHeight * resolutionWidth);
        }

        private void Update()
        {
            //if (sendMessage)
            //{
            //    sendMessage = false;
            //    PubCallback(message);
            //}
        }

        private void UpdateDepthBuffer(ScriptableRenderContext context, Camera camera)
        {
            if (update)
            {
                StartCoroutine(WaitForSeconds(.02f));//This starts the coroutine to lock this function 
                rect = imageCamera.rect;//pretty sure this sets the image camera view rect
                imageCamera.targetTexture = depthTexture;//sets the target to render to
                requests.Add(AsyncGPUReadback.Request(depthTexture));

                while (requests.Count > 0)
                {
                    // Check if the first entry in the queue is completed.
                    if (!requests[0].done)
                    {
                        // Detect out-of-order case (the second entry in the queue
                        // is completed before the first entry).
                        if (requests.Count > 1 && requests[1].done)
                        {
                            // We can't allow the out-of-order case, so force it to
                            // be completed now.
                            requests[0].WaitForCompletion();
                        }
                        else
                        {
                            // Nothing to do with the queue.
                            break;
                        }
                    }
         
                }

                //texture2D.ReadPixels(rect, 0, 0);//and then here it somehows reads the camera view rect into a textrure

                //Message setup. Not very important
                var multiArrayDims = new std_msgs.MultiArrayDimension[1] { new std_msgs.MultiArrayDimension("depthBuffer", size, sizeof(byte)) };
                var multiArrayLayout = new std_msgs.MultiArrayLayout(multiArrayDims, 0);
                var msg = new std_msgs.UInt8MultiArray(multiArrayLayout, requests[0].);
                message = msg;
            }
        }

        private void sendDepthbuffer(std_msgs.UInt8MultiArray msg)
        {
            Publish(msg);
        }

        //This coroutine locks the UpdateDepthBuffer to only update on a milisecond interval
        IEnumerator WaitForSeconds(float milis)
        {
            sendMessage = false;
            update = false;
            yield return new WaitForSecondsRealtime(milis);
            PubCallback(message);
            sendMessage = true;
            update = true;
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
