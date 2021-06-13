/// <summary>
/// Handles the camera frame parsing, and output drawing.
/// Author: Rowan Ramsey
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraFrameParser : MonoBehaviour
{
    [SerializeField] //'TextureParser' compute shader
    private ComputeShader textureParser;
    [SerializeField]//UI output for the depth image
    private RawImage depthOutput;
    [SerializeField]//UI output for color image
    private RawImage colorOutput;

    [HideInInspector]//A bool to determine when to send color frames
    public bool sendColorFrames = false;
    [HideInInspector]//A bool to determine when to send depth frames
    public bool sendDepthFrames = false;

    //Target to render the output of the attached camera to
    private RenderTexture targetTexture;
    //Texture to write color information to
    private RenderTexture outputColor;
    //Texture to write depth information to
    private RenderTexture outputDepth;

    //Handle for the computer shader kernel
    private int kernelHandle;

    public void Start()
    {
        if (textureParser != null)
        {
            kernelHandle = textureParser.FindKernel("ParseTexture");
        }

        targetTexture = GetComponent<Camera>().targetTexture;

        //Initializes both the color and depth sensor capture scripts and returns the target outputs for each
        outputColor = GetComponent<ColorSensorCapture>()?.Initialize().OutputColor;
        outputDepth = GetComponent<DepthSensorCapture>()?.Initialize().OutputDepth;
    }

    public void Update()
    {
        //Runs the computer shader to parse the rendered Frame
        FrameParser();

        //Draws the parsed frame to the color and depth UI outputs
        DrawOutput();

        //An attempt to 'sync' the sending of Color and Depth Frames
        sendColorFrames = true;
        sendDepthFrames = true;
    }

    private void DrawOutput()
    {
        //If the color UI output is not null, set the colorOutput to the color information recievd from the shader
        if (colorOutput)
        {
            colorOutput.texture = outputColor;
            colorOutput.Rebuild(CanvasUpdate.MaxUpdateValue);

            outputColor.enableRandomWrite = true;
            outputColor.Create();
        }

        //If the depth UI output is not null, set the depthOutput to the depth information recievd from the shader
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
        try//Try to run the compute shader, Log a warning, and then the error.
        {
            //Passes the camera output as color information to the 'colorTexture' variable in the shader
            textureParser.SetTexture(kernelHandle, "colorTexture", targetTexture, 0, RenderTextureSubElement.Default);
            //Passes the camera output as depth information to the 'depthTexture' variable in the shader
            textureParser.SetTexture(kernelHandle, "depthTexture", targetTexture, 0, RenderTextureSubElement.Depth);
            //Outputs for the color and depth
            textureParser.SetTexture(kernelHandle, "outputColor", outputColor, 0);
            textureParser.SetTexture(kernelHandle, "outputDepth", outputDepth, 0);
            //Frame information
            textureParser.SetInt("screenWidth", 1920);
            textureParser.SetInt("screenHeight", 1080);
            textureParser.SetInt("depthWidth", 1280);
            textureParser.SetInt("depthHeight", 720);
            uint x, y, z;
            textureParser.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
            //Actually runs the compute shader
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
