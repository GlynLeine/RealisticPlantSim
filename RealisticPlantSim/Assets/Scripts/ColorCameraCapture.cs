using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FFmpegOut;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ColorCameraCapture : CameraCapture
{
    [SerializeField]
    private RenderTexture cameraTarget;
    [SerializeField]
    private RawImage outputRenderer;

    private void Update()
    {
        GetComponent<Camera>().targetTexture = new RenderTexture(width, height, 24);
        //CamUpdate();
        outputRenderer.texture = GetComponent<Camera>().targetTexture;
        outputRenderer.Rebuild(CanvasUpdate.MaxUpdateValue);
    }

}
