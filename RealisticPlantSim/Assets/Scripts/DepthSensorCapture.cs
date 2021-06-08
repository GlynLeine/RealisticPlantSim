/// <summary>
/// Handles pushing the parsed depth information, to the FFmpeg pipeline for streaming
/// Author: Rowan Ramsey
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("Sensors/Depth Sensor")]
public class DepthSensorCapture : CameraCapture
{
    //The render texture that holds the depth information from the camera
    private RenderTexture outputDepth;

    CameraFrameParser frameParser;

    public DepthSensorCapture Initialize()
    {
        frameParser = GetComponent<CameraFrameParser>();
        outputDepth = new RenderTexture(streamWidth, streamHeight, 1, RenderTextureFormat.ARGBFloat);
        outputDepth.enableRandomWrite = true;
        outputDepth.Create();
        return this;
    }

    private void LateUpdate()
    {
        //A check to see if the the frameParser has succesfully parsed the depth frame information
        if (frameParser.sendDepthFrames)
        {
            //Pushes the parsed frame to the FFmpeg pipeline
            PushToPipe(outputDepth, streamURL, streamWidth, streamHeight);
            frameParser.sendDepthFrames = false;
        }
    }
    public RenderTexture OutputDepth
    {
        get { return outputDepth; }
        set { outputDepth = value; }
    }
}
