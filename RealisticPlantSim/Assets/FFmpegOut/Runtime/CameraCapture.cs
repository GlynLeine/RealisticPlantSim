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

        [SerializeField] int _colorStreamWidth = 1920;

        public int colorStreamWidth
        {
            get { return _colorStreamWidth; }
            set { _colorStreamWidth = value; }
        }

        [SerializeField] int _colorStreamHeight = 1080;

        public int colorStreamHeight
        {
            get { return _colorStreamHeight; }
            set { _colorStreamHeight = value; }
        }

        [SerializeField] int _depthStreamWidth = 1920;

        public int depthStreamWidth
        {
            get { return _depthStreamWidth; }
            set { _depthStreamWidth = value; }
        }

        [SerializeField] int _depthStreamHeight = 1080;

        public int depthStreamHeight
        {
            get { return _depthStreamHeight; }
            set { _depthStreamHeight = value; }
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

        private Dictionary<string, FFmpegSession> sessions = new Dictionary<string, FFmpegSession>();

        [SerializeField] string _colorStreamURL = "rtsp://localhost:8554/mystream";
        [SerializeField] string _depthStreamURL = "rtsp://localhost:8554/mystream";

        public string colorStreamURL
        {
            get { return _colorStreamURL; }
            set { _colorStreamURL = value; }
        }
        public string depthStreamURL
        {
            get { return _depthStreamURL; }
            set { _depthStreamURL = value; }
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
            _colorStreamWidth = Mathf.Max(8, _colorStreamWidth);
            _colorStreamHeight = Mathf.Max(8, _colorStreamHeight);

            _depthStreamWidth = Mathf.Max(8, _depthStreamWidth);
            _depthStreamHeight = Mathf.Max(8, _depthStreamHeight);
        }

        void OnDisable()
        {
            if (sessions[colorStreamURL] != null)
            {
                // Close and dispose the FFmpeg session.
                sessions[colorStreamURL].Close();
                sessions[colorStreamURL].Dispose();
                sessions[colorStreamURL] = null;
            }
            if (sessions[depthStreamURL] != null)
            {
                // Close and dispose the FFmpeg session.
                sessions[depthStreamURL].Close();
                sessions[depthStreamURL].Dispose();
                sessions[depthStreamURL] = null;
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
            sessions[colorStreamURL] = null;
            sessions[depthStreamURL] = null;
            // Sync with FFmpeg pipe thread at the end of every frame.
            for (var eof = new WaitForEndOfFrame(); ;)
            {
                yield return eof;
                if (sessions[colorStreamURL]!=null)
                    sessions[colorStreamURL].CompletePushFrames();
                else
                     Debug.LogWarning("Color Stream Session is not Initialized");

                if (sessions[depthStreamURL]!=null)
                    sessions[depthStreamURL].CompletePushFrames();
                else
                    Debug.LogWarning("Depth Stream Session is not Inialized");
            }
        }

        protected void PushToPipe(Texture texture, string url, int width, int height)
        {
            var _session = sessions[url];
            RTSPServerLoader.GetInstance();
            // Lazy initialization
            if (_session == null)
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
                sessions[url] = FFmpegSession.Create(
                    url,
                    width,
                    height,
                    _frameRate, preset,
                    _crfValue,
                    _maxBitrate
                );
                _session = sessions[url];

                _startTime = Time.time;
                _frameCount = 0;
                _frameDropCount = 0;
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
