using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using System.Linq;

namespace RosSharp.RosBridgeClient
{
    public class DepthBufferPublisher : UnityPublisher<std_msgs.UInt8MultiArray>
    {
        public string FrameId = "Test";

        [SerializeField]
        private Camera imageCamera;
        [SerializeField]
        private RenderTexture depthTexture;
        [SerializeField]
        private bool gpu = false;

        private int resolutionWidth = 1280;
        private int resolutionHeight = 720;
        private uint size;
        private std_msgs.UInt8MultiArray message;
        byte[] buffer;
        float[] floatBuffer;
        private bool update = true;

        private Texture2D texture2D;
        private Rect rect;
        int kernelHandle;
        public ComputeShader depthGetter;
        private ComputeBuffer depthData;



        protected override void Start()
        {
            base.Start();
            PubCallback += sendDepthbuffer;
            if (depthGetter != null)
            {
                kernelHandle = depthGetter.FindKernel("GetDepth");
            }
            else
            {
                gpu = false;
                Debug.LogWarning("Computer Shader is not set");
            }
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB4444, false);
            size = (uint)(resolutionHeight * resolutionWidth);
            imageCamera.targetTexture = depthTexture;//sets the target to render to.
            rect = imageCamera.rect;//pretty sure this sets the image camera view rect.
        }


        private void UpdateDepthBuffer(ScriptableRenderContext context, Camera camera)
        {
            if (update)
            {
                StartCoroutine(WaitForSeconds(.02f));//This starts the coroutine to lock this function

                if (!gpu)
                {
                    texture2D.ReadPixels(rect, 0, 0);//and then here it somehows reads the camera view rect into a textrure
                    buffer = texture2D.EncodeToPNG();
                }
                else
                {
                    buffer = new byte[size];
                    depthData = new ComputeBuffer((int)size, sizeof(byte));
                    depthGetter.SetTexture(kernelHandle, "depthTexture", depthTexture, 0, RenderTextureSubElement.Depth);
                    depthGetter.SetInt("bufferSize",(int)size);
                    depthGetter.SetBuffer(kernelHandle, "outputBuffer", depthData);
                    uint x, y, z;
                    depthGetter.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
                    depthGetter.Dispatch(kernelHandle, (int)(size/(4*x)), 1, 1);
                    depthData.GetData(buffer);
                    depthData.Dispose();
                }

                //Message setup. Not very important
                var multiArrayDims = new std_msgs.MultiArrayDimension[1] { new std_msgs.MultiArrayDimension("depthData", (uint)buffer.Length, sizeof(byte)) };
                var multiArrayLayout = new std_msgs.MultiArrayLayout(multiArrayDims, 0);
                var msg = new std_msgs.UInt8MultiArray(multiArrayLayout, buffer);
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
            update = false;
            yield return new WaitForSecondsRealtime(milis);
            PubCallback(message);
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
