using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraSensorCapture : CameraCapture
{
    [SerializeField]
    private ComputeShader depthGetter;

    private RenderTexture targetTexture;

    private RenderTexture outputDepth;
    private RenderTexture outputColor;
    private int kernelHandle;
    private uint size;
    [SerializeField]
    private RawImage depthOutput;
    [SerializeField]
    private RawImage colorOutput;

    private void Awake()
    {
        if (depthGetter != null)
        {
            kernelHandle = depthGetter.FindKernel("ParseTexture");
        }

        targetTexture = GetComponent<Camera>().targetTexture;
        //targetTexture.enableRandomWrite = true;
        //targetTexture.Create();

        outputColor = new RenderTexture(colorStreamWidth, colorStreamHeight, 16, RenderTextureFormat.ARGB32);
      

        outputDepth = new RenderTexture(depthStreamWidth, depthStreamHeight, 16, RenderTextureFormat.ARGB32);

    }

    private void Update()
    {
        //Debug.Log("Calls");

        outputColor.enableRandomWrite = true;
        outputColor.Create();

        outputDepth.enableRandomWrite = true;
        outputDepth.Create();

        FrameParser();
        PushToPipe(outputColor, colorStreamURL, colorStreamWidth, colorStreamHeight);
        colorOutput.texture = outputColor;
        colorOutput.Rebuild(CanvasUpdate.MaxUpdateValue);
    }
    private void FixedUpdate()
    {
        PushToPipe(outputDepth, depthStreamURL, depthStreamWidth, depthStreamHeight);
        depthOutput.texture = outputDepth;
        depthOutput.Rebuild(CanvasUpdate.MaxUpdateValue);
    }

    void FrameParser()
    {
        depthGetter.SetTexture(kernelHandle, "colorTexture", targetTexture, 0, RenderTextureSubElement.Default);
        depthGetter.SetTexture(kernelHandle, "depthTexture", targetTexture, 0, RenderTextureSubElement.Depth);
        depthGetter.SetTexture(kernelHandle, "outputColor", outputColor, 0);
        depthGetter.SetTexture(kernelHandle, "outputDepth", outputDepth, 0);
        uint x, y, z;
        depthGetter.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
        depthGetter.Dispatch(kernelHandle, (int)(colorStreamWidth / x), (int)(colorStreamHeight / y), 1);
    }
}
