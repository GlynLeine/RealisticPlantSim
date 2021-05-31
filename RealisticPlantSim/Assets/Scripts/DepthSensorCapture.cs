using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("Sensors/Depth Sensor")]
public class DepthSensorCapture : CameraCapture
{
    private RenderTexture outputDepth;
    [SerializeField]
    private RawImage depthOutput;
    StreamSettings settings;

    public DepthSensorCapture Initialize()
    {
        settings = GetComponent<StreamSettings>();
        outputDepth = new RenderTexture(streamWidth, streamHeight, 16, RenderTextureFormat.ARGB32);
        outputDepth.enableRandomWrite = true;
        outputDepth.Create();
        return this;
    }

    private void Update()
    {
        if (settings.sendDepthFrames)
        {
            PushToPipe(outputDepth, streamURL, streamWidth, streamHeight);

            depthOutput.texture = outputDepth;
            depthOutput.Rebuild(CanvasUpdate.MaxUpdateValue);

            outputDepth.enableRandomWrite = true;
            outputDepth.Create();

            settings.OutputDepth = outputDepth;
        }
    }
    public RenderTexture OutputDepth
    {
        get { return outputDepth; }
        set { outputDepth = value; }
    }
}
