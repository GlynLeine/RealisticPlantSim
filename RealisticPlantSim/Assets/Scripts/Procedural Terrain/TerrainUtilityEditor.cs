using System;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(TerrainUtility))]
public class TerrainUtilityEditor : Editor
{
    SerializedProperty _plantsPerUnit;

    int plantsToSpawn;

    IETACalculator eta = null;

    void OnEnable()
    {
        _plantsPerUnit = serializedObject.FindProperty("plantsPerUnit");
    }

    public override void OnInspectorGUI()
    {
        if (eta == null)
        {
            eta = new ETACalculator(30, 15);
        }
        serializedObject.Update();
        TerrainUtility terrainUtility = (TerrainUtility)target;
        TerrainGenerator terrainGenerator = terrainUtility.GetComponent<TerrainGenerator>();

        PlantGenerator plantGenerator = terrainUtility.GetComponent<PlantGenerator>();

        //Terrain
        GUILayout.Label("Terrain generation");


        terrainGenerator.terrainWidth = EditorGUILayout.FloatField(new GUIContent("Terrain size X"), terrainGenerator.terrainWidth);

        plantGenerator.xMinVal = -(terrainGenerator.terrainWidth / 2);
        plantGenerator.xMaxVal = terrainGenerator.terrainWidth / 2;
        plantGenerator.zMinVal = -(terrainGenerator.terrainLength / 2);
        plantGenerator.zMaxVal = terrainGenerator.terrainLength / 2;


        terrainGenerator.terrainLength = EditorGUILayout.FloatField(new GUIContent("Terrain size Z"), terrainGenerator.terrainLength);


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create terrain"))
        {
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.buildTerrain();
        }

        if (GUILayout.Button("Delete terrain"))
        {
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.deleteTerrain();
        }

        GUILayout.EndHorizontal();


        //Plants
        GUILayout.Label("Plant generation");

        EditorGUILayout.PropertyField(_plantsPerUnit);

        plantsToSpawn = (int)Math.Round(_plantsPerUnit.floatValue * terrainGenerator.terrainLength * terrainGenerator.terrainWidth);
        GUILayout.Label($"This will spawn {plantsToSpawn} plants");
        plantGenerator.amountOfPlantsToSpawn = plantsToSpawn;

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate plants"))
        {
            plantGenerator.StartCoroutine(plantGenerator.spawnPlantsInXZRange());

            eta.Reset();
        }

        if (GUILayout.Button("Delete plants"))
        {
            plantGenerator.deleteAllPlants();
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Stop generator"))
        {
            plantGenerator.StopAllCoroutines();
            plantGenerator.generatingPlants = false;
        }

        if (plantGenerator.generatingPlants)
        {
            float percentageDone = (float) plantGenerator.currentPlant / (float) plantGenerator.amountOfPlantsToSpawn;
            eta.Update(percentageDone);
            EditorGUILayout.LabelField($"Generating plants... {plantGenerator.currentPlant} / {plantGenerator.amountOfPlantsToSpawn}");
            if(eta.ETAIsAvailable)
            {
                EditorGUILayout.LabelField($"Estimated time left: {eta.ETR.Hours}:{eta.ETR.Minutes}:{eta.ETR.Seconds}");
            } else
            {
                EditorGUILayout.LabelField($"Calculating time remaining...");

            }
            EditorGUILayout.LabelField("Don't click away from Unity, while clicked away the generator can't generate plants!");
            EditorGUILayout.LabelField("Tip! Look away from the field with the scene view. The generation will go faster");

        }



        serializedObject.ApplyModifiedProperties();
    }
}
