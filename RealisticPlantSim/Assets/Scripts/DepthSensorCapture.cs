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
    StreamSettings settings;

    public DepthSensorCapture Initialize()
    {
        settings = GetComponent<StreamSettings>();
        outputDepth = new RenderTexture(streamWidth, streamHeight, 1, RenderTextureFormat.ARGBFloat);
        outputDepth.enableRandomWrite = true;
        outputDepth.Create();
        return this;
    }

    private void Update()
    {
        if (settings.sendDepthFrames)
        {
            PushToPipe(outputDepth, streamURL, streamWidth, streamHeight);
            settings.sendDepthFrames = false;
        }
    }
    public RenderTexture OutputDepth
    {
        get { return outputDepth; }
        set { outputDepth = value; }
    }
}
