using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(PlantGenerator))]
public class PlantGeneratorEditor : Editor
{
    SerializedProperty _editorTimeGeneration;
    SerializedProperty _plantsPerChunk;
    SerializedProperty _plantSpawnSettings;
    SerializedProperty _minLimit;
    SerializedProperty _maxLimit;
    SerializedProperty _xMinVal;
    float xMinVal;
    SerializedProperty _zMinVal;
    float zMinVal;
    SerializedProperty _xMaxVal;
    float xMaxVal;
    SerializedProperty _zMaxVal;
    float zMaxVal;




    void OnEnable()
    {
        _editorTimeGeneration = serializedObject.FindProperty("editorTimeGeneration");
        _plantsPerChunk = serializedObject.FindProperty("plantsPerChunk");
        _plantSpawnSettings = serializedObject.FindProperty("plantSpawnSettings");
        _minLimit = serializedObject.FindProperty("minLimit");
        _maxLimit = serializedObject.FindProperty("maxLimit");
        _xMinVal = serializedObject.FindProperty("xMinVal");
        xMinVal = _xMinVal.floatValue;
        _zMinVal = serializedObject.FindProperty("zMinVal");
        zMinVal = _zMinVal.floatValue;
        _xMaxVal = serializedObject.FindProperty("xMaxVal");
        xMaxVal = _xMaxVal.floatValue;
        _zMaxVal = serializedObject.FindProperty("zMaxVal");
        zMaxVal = _zMaxVal.floatValue;

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_editorTimeGeneration);
        EditorGUILayout.PropertyField(_plantsPerChunk);
        EditorGUILayout.PropertyField(_plantSpawnSettings);

        // Show default inspector property editor
        if (_editorTimeGeneration.boolValue == true)
        {
            EditorGUILayout.PropertyField(_minLimit);
            EditorGUILayout.PropertyField(_maxLimit);

            EditorGUILayout.LabelField("X Min Value:", xMinVal.ToString());
            EditorGUILayout.LabelField("X Max Value:", xMaxVal.ToString());
            EditorGUILayout.MinMaxSlider("X min/max", ref xMinVal, ref xMaxVal, _minLimit.floatValue, _maxLimit.floatValue);
            _xMinVal.floatValue = xMinVal;
            _xMaxVal.floatValue = xMaxVal;

            //Editor time code
            if (GUILayout.Button("Generate plants"))
            {

                //Generate the plants
            }
        }



        serializedObject.ApplyModifiedProperties();




    }
}
