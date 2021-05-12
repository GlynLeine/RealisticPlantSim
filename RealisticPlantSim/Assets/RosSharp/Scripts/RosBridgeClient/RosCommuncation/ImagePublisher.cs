/*
© CentraleSupelec, 2017
Author: Dr. Jeremy Fix (jeremy.fix@centralesupelec.fr)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

// Adjustments to new Publication Timing and Execution Framework 
// © Siemens AG, 2018, Dr. Martin Bischoff (martin.bischoff@siemens.com)

using UnityEngine;
using UnityEngine.Rendering;

namespace RosSharp.RosBridgeClient
{
    public class ImagePublisher : UnityPublisher<MessageTypes.Sensor.CompressedImage>
    {
        public Camera ImageCamera;
        public string FrameId = "Camera";
        public int resolutionWidth = 640;
        public int resolutionHeight = 480;
        [Range(0, 100)]
        public int qualityLevel = 50;

        private MessageTypes.Sensor.CompressedImage message;
        private Texture2D texture2D;
        private Rect rect;

        protected override void Start()
        {
            base.Start();
            InitializeGameObject();
            InitializeMessage();
            //RenderPipelineManager.endCameraRendering += UpdateImage;
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            ImageCamera.targetTexture = new RenderTexture(resolutionWidth,resolutionHeight,24);
            //ImageCamera.targetTexture.width = resolutionWidth;
            //ImageCamera.targetTexture.height = resolutionHeight;
            rect = ImageCamera.rect;
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.CompressedImage();
            message.header.frame_id = FrameId;
            message.format = "jpeg";
        }

        private void UpdateMessage()
        {
            message.header.Update();
            texture2D.ReadPixels(rect, 0, 0);
            message.data = texture2D.EncodeToJPG(qualityLevel);
            Publish(message);
        }

        private void UpdateImage(ScriptableRenderContext context, Camera camera)
        {
            if (texture2D != null && camera == this.ImageCamera)
            {
                UpdateMessage();
            }
        }

        void OnEnable()
        {
            RenderPipelineManager.endCameraRendering += UpdateImage;
        }
        void OnDisable()
        {
            RenderPipelineManager.endCameraRendering -= UpdateImage;
        }
    }
}