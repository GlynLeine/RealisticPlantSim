using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DepthCameraCapture : CameraCapture
{
    [SerializeField]
    private ComputeShader depthGetter;
    private RenderTexture depthTexture;
    private Texture outputDepth;
    private Texture outputColor;
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
            kernelHandle = depthGetter.FindKernel("GetDepth");
        }
        outputDepth = new Texture2D(width,height);
        outputColor = new Texture2D(width, height);
    }

    private void Update()
    {
        FrameParser();
        PushToPipe(outputColor);
        PushToPipe(outputDepth);
        colorOutput.texture = outputColor; 
        colorOutput.Rebuild(CanvasUpdate.MaxUpdateValue);
        depthOutput.texture = outputDepth; 
        depthOutput.Rebuild(CanvasUpdate.MaxUpdateValue);
    }

    void FrameParser()
    {
        depthGetter.SetTexture(kernelHandle, "depthTexture", depthTexture, 0, RenderTextureSubElement.Depth);
        depthGetter.SetTexture(kernelHandle, "colorTexture", depthTexture, 0, RenderTextureSubElement.Color);
        depthGetter.SetTexture(kernelHandle, "outputColor", outputDepth, 0);
        depthGetter.SetTexture(kernelHandle, "outputDepth", outputColor, 0);
        uint x, y, z;
        depthGetter.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
        depthGetter.Dispatch(kernelHandle, (int)(width / x), (int)(height / y), 1);
    }
}
