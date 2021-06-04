using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StreamSettings : MonoBehaviour
{
    [SerializeField]
    private ComputeShader textureParser;
    [SerializeField]
    private RawImage depthOutput;
    [SerializeField]
    private RawImage colorOutput;

    [HideInInspector]
    public bool sendColorFrames = false;

    [HideInInspector]
    public bool sendDepthFrames = false;

    private RenderTexture targetTexture;
    private RenderTexture outputDepth;
    private RenderTexture outputColor;

    private int kernelHandle;
    private uint size;

    public void Start()
    {
        if (textureParser != null)
        {
            kernelHandle = textureParser.FindKernel("ParseTexture");
        }

        targetTexture = GetComponent<Camera>().targetTexture;
        outputColor = GetComponent<ColorSensorCapture>()?.Initialize().OutputColor;
        outputDepth = GetComponent<DepthSensorCapture>()?.Initialize().OutputDepth;
    }

    public void Update()
    {
        FrameParser();
        DrawOutput();
        sendColorFrames = true;
        sendDepthFrames = true;
    }

    private void DrawOutput()
    {
        if (colorOutput)
        {
            colorOutput.texture = outputColor;
            colorOutput.Rebuild(CanvasUpdate.MaxUpdateValue);

            outputColor.enableRandomWrite = true;
            outputColor.Create();
        }

        if (depthOutput)
        {
            depthOutput.texture = outputDepth;
            depthOutput.Rebuild(CanvasUpdate.MaxUpdateValue);

            outputDepth.enableRandomWrite = true;
            outputDepth.Create();
        }
    }

    public void FrameParser()
    {
        try
        {
            textureParser.SetTexture(kernelHandle, "colorTexture", targetTexture, 0, RenderTextureSubElement.Default);
            textureParser.SetTexture(kernelHandle, "depthTexture", targetTexture, 0, RenderTextureSubElement.Depth);
            textureParser.SetTexture(kernelHandle, "outputColor", outputColor, 0);
            textureParser.SetTexture(kernelHandle, "outputDepth", outputDepth, 0);
            textureParser.SetInt("screenWidth", 1920);
            textureParser.SetInt("screenHeight", 1080);
            textureParser.SetInt("depthWidth", 1280);
            textureParser.SetInt("depthHeight", 720);
            uint x, y, z;
            textureParser.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
            textureParser.Dispatch(kernelHandle, (int)(1920 / x), (int)(1080 / y), 1);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Something went wrong in the FrameParser");
            Debug.LogError(e);
        }
    }

    public RenderTexture OutputColor
    {
        get { return outputColor; }
        set { outputColor = value; }
    }

    public RenderTexture OutputDepth
    {
        get { return outputDepth; }
        set { outputDepth = value; }
    }


}
