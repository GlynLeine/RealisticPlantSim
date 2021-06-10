/// <summary>
/// Handles pushing the parsed color information, to the FFmpeg pipeline for streaming
/// Author: Rowan Ramsey
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("Sensors/Color Sensor")]
public class ColorSensorCapture : CameraCapture
{
    //The render texture that holds the color information from the camera
    private RenderTexture outputColor;

    CameraFrameParser frameParser;

    public ColorSensorCapture Initialize()
    {
        frameParser = GetComponent<CameraFrameParser>();
        outputColor = new RenderTexture(streamWidth, streamHeight, 1, RenderTextureFormat.ARGBFloat);
        outputColor.enableRandomWrite = true;
        outputColor.Create();
        return this;
    }

    private void LateUpdate()
    {
        //A check to see if the the frameParser has succesfully parsed the color frame information
        if (frameParser.sendColorFrames)
        {
            //Pushes the parsed frame to the FFmpeg pipeline
            PushToPipe(outputColor, streamURL, streamWidth, streamHeight);
            frameParser.sendColorFrames = false;
        }
    }

    public RenderTexture OutputColor
    {
        get { return outputColor; }
        set { outputColor = value; }
    }
}


