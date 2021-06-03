using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class StreamSettings : MonoBehaviour
{
    [SerializeField]
    private ComputeShader depthGetter;

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
        if (depthGetter != null)
        {
            kernelHandle = depthGetter.FindKernel("ParseTexture");
        }

        targetTexture = GetComponent<Camera>().targetTexture;

        var colorSensor = GetComponent<ColorSensorCapture>()?.Initialize();
        var depthSensor = GetComponent<DepthSensorCapture>()?.Initialize();
        outputColor = colorSensor.OutputColor;
        outputDepth = depthSensor.OutputDepth;
    }

    public void Update()
    {
        FrameParser();
        sendColorFrames = true;
        sendDepthFrames = true;
    }

    public void FrameParser()
    {
        depthGetter.SetTexture(kernelHandle, "colorTexture", targetTexture, 0, RenderTextureSubElement.Default);
        depthGetter.SetTexture(kernelHandle, "depthTexture", targetTexture, 0, RenderTextureSubElement.Depth);
        depthGetter.SetTexture(kernelHandle, "outputColor", outputColor, 0);
        depthGetter.SetTexture(kernelHandle, "outputDepth", outputDepth, 0);
        uint x, y, z;
        depthGetter.GetKernelThreadGroupSizes(kernelHandle, out x, out y, out z);
        depthGetter.Dispatch(kernelHandle, (int)(1920 / x), (int)(1080 / y), 1);
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
