// FFmpegOut - FFmpeg video encoding plugin for Unity
// https://github.com/keijiro/KlakNDI

using UnityEngine;
using UnityEditor;
using System.Linq;

namespace FFmpegOut
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(CameraCapture))]
    public class CameraCaptureEditor : Editor
    {
        SerializedProperty _colorStreamWidth;
        SerializedProperty _colorStreamHeight;
        SerializedProperty _depthStreamWidth;
        SerializedProperty _depthStreamHeight;
        SerializedProperty _preset;
        SerializedProperty _frameRate;
        SerializedProperty _colorStreamURL;
        SerializedProperty _depthStreamURL;
        SerializedProperty _crfValue;
        SerializedProperty _maxBitrate;


        GUIContent[] _presetLabels;
        int[] _presetOptions;

        // It shows the render format options when:
        // - Editing multiple objects.
        // - No target texture is specified in the camera.
        bool ShouldShowFormatOptions
        {
            get {
                if (targets.Length > 1) return true;
                var camera = ((Component)target).GetComponent<Camera>();
                return camera.targetTexture == null;
            }
        }

        void OnEnable()
        {
            _colorStreamWidth = serializedObject.FindProperty("_colorStreamWidth");
            _colorStreamHeight = serializedObject.FindProperty("_colorStreamHeight");
            _depthStreamWidth = serializedObject.FindProperty("_depthStreamWidth");
            _depthStreamHeight = serializedObject.FindProperty("_depthStreamHeight");
            _preset = serializedObject.FindProperty("_preset");
            _frameRate = serializedObject.FindProperty("_frameRate");
            _colorStreamURL = serializedObject.FindProperty("_colorStreamURL");
            _depthStreamURL = serializedObject.FindProperty("_depthStreamURL");
            _crfValue = serializedObject.FindProperty("_crfValue");
            _maxBitrate = serializedObject.FindProperty("_maxBitrate");



            var presets = FFmpegPreset.GetValues(typeof(FFmpegPreset));
            _presetLabels = presets.Cast<FFmpegPreset>().
                Select(p => new GUIContent(p.GetDisplayName())).ToArray();
            _presetOptions = presets.Cast<int>().ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ShouldShowFormatOptions)
            {
                EditorGUILayout.PropertyField(_colorStreamWidth);
                EditorGUILayout.PropertyField(_colorStreamHeight);
                EditorGUILayout.PropertyField(_depthStreamWidth);
                EditorGUILayout.PropertyField(_depthStreamHeight);
            }

            EditorGUILayout.IntPopup(_preset, _presetLabels, _presetOptions);
            EditorGUILayout.PropertyField(_frameRate);
            EditorGUILayout.PropertyField(_colorStreamURL);
            EditorGUILayout.PropertyField(_depthStreamURL);
            EditorGUILayout.PropertyField(_crfValue, new GUIContent("CRF Value (0-51) higher is lower quality"));
            EditorGUILayout.PropertyField(_maxBitrate, new GUIContent("Max bitrate (Kbps)"));


            serializedObject.ApplyModifiedProperties();
        }
    }
}
