using UnityEngine;
using UnityEditor;
using static PlantGenerator;
using System;

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
        PlantGenerator plantGenerator = (PlantGenerator)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(_plantSpawnSettings);

        foreach(PlantSpawnSettings spawnSetting in plantGenerator.plantSpawnSettings)
        {
            AbstractPlacementStrategy placementStrategy;
            if (spawnSetting.placementStrategy is AbstractPlacementStrategy)
            {
                placementStrategy = Activator.CreateInstance(spawnSetting.placementStrategy.GetClass()) as AbstractPlacementStrategy;
            }
            else
            {
                throw new Exception($"[PlantGenerator] The Random Placement script on plant #{plantGenerator.plantSpawnSettings.IndexOf(spawnSetting)} does not extend from AbstractPlacementStrategy!");
            }
            placementStrategy.OnInspectorGUI();
        }


        EditorGUILayout.PropertyField(_xMinVal, new GUIContent("Minimum X bound"));
        EditorGUILayout.PropertyField(_xMaxVal, new GUIContent("Maximum X bound"));

        EditorGUILayout.PropertyField(_zMinVal, new GUIContent("Minimum Z bound"));
        EditorGUILayout.PropertyField(_zMaxVal, new GUIContent("Maximum Z bound"));

        EditorGUILayout.PropertyField(_amountOfPlantsToSpawn, new GUIContent("Amount of plants to spawn"));


        if (GUILayout.Button("Generate plants"))
        {
            //Generate the plants
            plantGenerator.StartCoroutine(plantGenerator.spawnPlantsInXZRange());
        }

        if(GUILayout.Button("Cancel generator"))
        {
            plantGenerator.StopAllCoroutines();
        }

        if(GUILayout.Button("Delete all plants"))
        {
            plantGenerator.deleteAllPlants();
        }



        serializedObject.ApplyModifiedProperties();




    }
}
