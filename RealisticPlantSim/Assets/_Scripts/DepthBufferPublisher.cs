using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using System.Linq;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
    public class DepthBufferPublisher : UnityPublisher<std_msgs.UInt8MultiArray>
    {
        public string FrameId = "Test";
        [SerializeField]
        private int resolutionWidth = 1280;
        [SerializeField]
        private int resolutionHeight = 720;
        [SerializeField]
        private Camera imageCamera;
        [SerializeField]
        private bool gpu = false;
        [SerializeField]
        private RawImage meshRenderer;
        [SerializeField]
        private ComputeShader depthGetter;

        private RenderTexture depthTexture;
        private RenderTexture outputTexture;

        private uint size;
        private std_msgs.UInt8MultiArray message;
        byte[] buffer;
        private bool update = true;

        private Texture2D texture2D;
        private Rect rect;
        private int kernelHandle;


        protected override void Start()
        {
            base.Start();
            PubCallback += sendDepthbuffer;

            if (depthGetter != null)
            {
                kernelHandle = depthGetter.FindKernel("ParseTexture");
            }
            else
            {
                gpu = false;
                Debug.LogWarning("Computer Shader is not set");
            }

            //gpu output
            outputTexture = new RenderTexture(resolutionWidth, resolutionHeight, 0);
            outputTexture.enableRandomWrite = true;
            outputTexture.Create();

            //cpu output
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.ARGB4444, false);
            size = (uint)(resolutionHeight * resolutionWidth);
            depthTexture = imageCamera.targetTexture;//sets the target to render to.
            rect = imageCamera.rect;//pretty sure this sets the image camera view rect.
        }


        private void UpdateDepthBuffer(ScriptableRenderContext context, Camera camera)
        {
            if (update)
            {
                //StartCoroutine(WaitForSeconds(.2f));//This starts the coroutine to lock this function

                if (!gpu)
                {
                    texture2D.ReadPixels(rect, 0, 0);//and then here it somehows reads the camera view rect into a textrure
                    buffer = texture2D.EncodeToPNG();
                }
                else
                {
                    depthGetter.SetTexture(kernelHandle, "depthTexture", depthTexture, 0, RenderTextureSubElement.Depth);
                    depthGetter.SetTexture(kernelHandle, "outputTexture", outputTexture, 0, RenderTextureSubElement.Color);
                    depthGetter.SetInt("bufferSize",(int)size);
                    uint x, y, z;
                    depthGetter.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
                    depthGetter.Dispatch(kernelHandle, (int)(resolutionWidth/x), (int)(resolutionWidth/y), 1);
                    meshRenderer.texture = outputTexture;
                    meshRenderer.Rebuild(CanvasUpdate.MaxUpdateValue);

                }
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
