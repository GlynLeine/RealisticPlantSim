using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("Sensors/Color Sensor")]
public class ColorSensorCapture : CameraCapture
{
    private RenderTexture outputColor;
    [SerializeField]
    private RawImage colorOutput;
    StreamSettings settings;

    public ColorSensorCapture Initialize()
    {
        settings = GetComponent<StreamSettings>();
        outputColor = new RenderTexture(streamWidth, streamHeight, 16, RenderTextureFormat.ARGB32);
        outputColor.enableRandomWrite = true;
        outputColor.Create();
        return this;
    }

    private void Update()
    {
        if (settings.sendColorFrames)
        {
            PushToPipe(outputColor, streamURL, streamWidth, streamHeight);
            settings.sendColorFrames = false;

            colorOutput.texture = outputColor;
            colorOutput.Rebuild(CanvasUpdate.MaxUpdateValue);

            outputColor.enableRandomWrite = true;
            outputColor.Create();

            settings.OutputColor = outputColor;
        }
    }

    public RenderTexture OutputColor
    {
        get { return outputColor; }
        set { outputColor = value; }
    }
}


