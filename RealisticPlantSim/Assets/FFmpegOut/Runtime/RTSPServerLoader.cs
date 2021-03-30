using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class RTSPServerLoader
{
    public static RTSPServerLoader instance;

    public bool RTSPServerloaded = false;
    private Process process;
    private StreamWriter messageStream;

    public RTSPServerLoader()
    {
        try
        {
            process = new Process();
            process.EnableRaisingEvents = false;
            process.StartInfo.FileName = Application.dataPath + "./RTSPServer/rtsp-simple-server.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += new DataReceivedEventHandler(DataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceived);
            process.Start();
            process.BeginOutputReadLine();
            messageStream = process.StandardInput;

            UnityEngine.Debug.Log("Starting RTSP Server");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Unable to launch app: " + e.Message);
        }
    }

    IEnumerator WaitForServerToStart()
    {
        yield return new WaitForSeconds(2);
        RTSPServerloaded = true;
        UnityEngine.Debug.Log("Started RTSP Server");

    }

    void DataReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        UnityEngine.Debug.Log(eventArgs.Data);
    }


    void ErrorReceived(object sender, DataReceivedEventArgs eventArgs)
    {
        UnityEngine.Debug.LogError(eventArgs.Data);
    }


    public void Kill()
    {
        if (process != null && !process.HasExited)
        {
            process.Kill();
        }
    }


    public static RTSPServerLoader GetInstance()
    {
        if (instance == null)
        {
            instance = new RTSPServerLoader();
            UnityEngine.Debug.Log("Creating new instance of RTSPServerLoader");
        }
        return instance;
    }
}
