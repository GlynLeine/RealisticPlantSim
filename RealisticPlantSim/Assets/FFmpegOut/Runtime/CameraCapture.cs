// FFmpegOut - FFmpeg video encoding plugin for Unity
// https://github.com/keijiro/KlakNDI

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace FFmpegOut
{
    [AddComponentMenu("FFmpegOut/Camera Capture")]
    public class CameraCapture : MonoBehaviour
    {
        #region Public properties

        [SerializeField] int _streamWidth = 1920;

        public int streamWidth
        {
            get { return _streamWidth; }
            set { _streamWidth = value; }
        }

        [SerializeField] int _streamHeight = 1080;

        public int streamHeight
        {
            get { return _streamHeight; }
            set { _streamHeight = value; }
        }

        [SerializeField] FFmpegPreset _preset;

        public FFmpegPreset preset
        {
            get { return _preset; }
            set { _preset = value; }
        }

        [SerializeField] float _frameRate = 30;

        public float frameRate
        {
            get { return _frameRate; }
            set { _frameRate = value; }
        }

        [SerializeField] string _streamURL = "rtsp://localhost:8554/mystream";

        public string streamURL
        {
            get { return _streamURL; }
            set { _streamURL = value; }
        }

        [SerializeField] int _crfValue = 23;

        public int crfValue
        {
            get { return _crfValue; }
            set { _crfValue = value; }
        }

        [SerializeField] int _maxBitrate = 20000;

        public int maxBitrate
        {
            get { return _maxBitrate; }
            set { _maxBitrate = value; }
        }


        #endregion

        #region Private members
        FFmpegSession _session;
        RenderTexture _tempRT;
        GameObject _blitter;

        RenderTextureFormat GetTargetFormat(Camera camera)
        {
            return camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        }

        int GetAntiAliasingLevel(Camera camera)
        {
            return camera.allowMSAA ? QualitySettings.antiAliasing : 1;
        }

        #endregion

        #region Time-keeping variables

        int _frameCount;
        float _startTime;
        int _frameDropCount;

        float FrameTime
        {
            get { return _startTime + (_frameCount - 0.5f) / _frameRate; }
        }

        void WarnFrameDrop()
        {
            if (++_frameDropCount != 10) return;

            Debug.LogWarning(
                "Significant frame droppping was detected. This may introduce " +
                "time instability into output video. Decreasing the recording " +
                "frame rate is recommended."
            );
        }

        #endregion

        #region MonoBehaviour implementation

        void OnValidate()
        {
            _streamWidth = Mathf.Max(8, _streamWidth);
            _streamHeight = Mathf.Max(8, _streamHeight);
        }

        void OnDisable()
        {
            if (_session!= null)
            {
                // Close and dispose the FFmpeg session.
                _session.Close();
                _session.Dispose();
                _session = null;
            }

            if (_tempRT != null)
            {
                // Dispose the frame texture.
                GetComponent<Camera>().targetTexture = null;
                Destroy(_tempRT);
                _tempRT = null;
            }

            if (_blitter != null)
            {
                // Destroy the blitter game object.
                Destroy(_blitter);
                _blitter = null;
            }
        }

        IEnumerator Start()
        {
            _session = null;
            // Sync with FFmpeg pipe thread at the end of every frame.
            for (var eof = new WaitForEndOfFrame(); ;)
            {
                yield return eof;
                if (_session != null)
                    _session.CompletePushFrames();
                else
                     Debug.LogWarning("Stream Session is not Initialized");
            }
        }

        protected void PushToPipe(Texture texture, string url, int width, int height)
        {
            RTSPServerLoader loader = RTSPServerLoader.GetInstance();
            if(!loader.CoroutineStarted)
            {
                StartCoroutine(loader.WaitForServerToStart());
            }
            // Lazy initialization
            if (_session == null && loader.RTSPServerloaded)
            {
                Debug.Log("Creating Session: "+url);
                // Give a newly created temporary render texture to the camera
                // if it's set to render to a screen. Also create a blitter
                // object to keep frames presented on the screen.
                if (texture == null)
                {
                    Debug.LogError("Texture is null");
                }

                // Start an FFmpeg session.
                _session = FFmpegSession.Create(
                    url,
                    width,
                    height,
                    _frameRate, preset,
                    _crfValue,
                    _maxBitrate
                );

                _startTime = Time.time;
                _frameCount = 0;
                _frameDropCount = 0;
            }

            if(_session == null)
            {
                return;
            }

            var gap = Time.time - FrameTime;
            var delta = 1 / _frameRate;

            if (gap < 0)
            {
                // Update without frame data.
                _session.PushFrame(null);
            }
            else if (gap < delta)
            {
                // Single-frame behind from the current time:
                // Push the current frame to FFmpeg.
                _session.PushFrame(texture);
                _frameCount++;
            }
            else if (gap < delta * 2)
            {
                // Two-frame behind from the current time:
                // Push the current frame twice to FFmpeg. Actually this is not
                // an efficient way to catch up. We should think about
                // implementing frame duplication in a more proper way. #fixme
                _session.PushFrame(texture);
                _session.PushFrame(texture);
                _frameCount += 2;
            }
            else
            {
                // Show a warning message about the situation.
                WarnFrameDrop();

                // Push the current frame to FFmpeg.
                _session.PushFrame(texture);

                // Compensate the time delay.
                _frameCount += Mathf.FloorToInt(gap * _frameRate);
            }
        }

        #endregion

    }




}
