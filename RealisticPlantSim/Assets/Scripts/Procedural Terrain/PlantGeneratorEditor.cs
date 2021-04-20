using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(PlantGenerator))]
public class PlantGeneratorEditor : Editor
{
    SerializedProperty _plantSpawnSettings;
    SerializedProperty _xMinVal;
    SerializedProperty _zMinVal;
    SerializedProperty _xMaxVal;
    SerializedProperty _zMaxVal;
    SerializedProperty _amountOfPlantsToSpawn;




    void OnEnable()
    {
        _plantSpawnSettings = serializedObject.FindProperty("plantSpawnSettings");
        _xMinVal = serializedObject.FindProperty("xMinVal");
        _zMinVal = serializedObject.FindProperty("zMinVal");
        _xMaxVal = serializedObject.FindProperty("xMaxVal");
        _zMaxVal = serializedObject.FindProperty("zMaxVal");
        _amountOfPlantsToSpawn = serializedObject.FindProperty("amountOfPlantsToSpawn");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_plantSpawnSettings);

        EditorGUILayout.PropertyField(_xMinVal, new GUIContent("Minimum X bound"));
        EditorGUILayout.PropertyField(_xMaxVal, new GUIContent("Maximum X bound"));

        EditorGUILayout.PropertyField(_zMinVal, new GUIContent("Minimum Z bound"));
        EditorGUILayout.PropertyField(_zMaxVal, new GUIContent("Maximum Z bound"));

        EditorGUILayout.PropertyField(_amountOfPlantsToSpawn, new GUIContent("Amount of plants to spawn"));


        if (GUILayout.Button("Generate plants"))
        {
            //Generate the plants
            PlantGenerator plantGenerator = (PlantGenerator)target;
            plantGenerator.StartCoroutine(plantGenerator.spawnPlantsInXZRange());
        }

        if(GUILayout.Button("Delete all plants"))
        {
            PlantGenerator plantGenerator = (PlantGenerator)target;
            plantGenerator.deleteAllPlants();
        }



        serializedObject.ApplyModifiedProperties();




    }
}
