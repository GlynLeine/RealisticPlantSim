using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CanEditMultipleObjects]
[CustomEditor(typeof(TerrainUtility))]
public class TerrainUtilityEditor : Editor
{
    SerializedProperty _plantsPerUnit;
    SerializedProperty _chunksPerFrame;
    SerializedProperty _plantsPerFrame;


    int plantsToSpawn;


    void OnEnable()
    {
        _plantsPerUnit = serializedObject.FindProperty("plantsPerUnit");
        _chunksPerFrame = serializedObject.FindProperty("chunksPerFrame");
        _plantsPerFrame = serializedObject.FindProperty("plantsPerFrame");
    }

    //private void OnSceneGUI()
    //{
    //    TerrainUtility terrainUtility = (TerrainUtility)target;
    //    PlantGenerator plantGenerator = terrainUtility.GetComponent<PlantGenerator>();

    //    if (plantGenerator.generatingPlants)
    //    {
    //        if (Event.current.type == EventType.Repaint)
    //        {
    //            SceneView.RepaintAll();
    //        }
    //    }

    //}

    public override void OnInspectorGUI()
    {
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

        EditorGUILayout.PropertyField(_chunksPerFrame);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create terrain"))
        {
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.buildTerrain(_chunksPerFrame.intValue);
        }

        if (GUILayout.Button("Delete terrain"))
        {
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.deleteTerrain();
            EditorSceneManager.MarkAllScenesDirty();
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        GUILayout.EndHorizontal();


        //Plants
        GUILayout.Label("Plant generation");

        EditorGUILayout.PropertyField(_plantsPerUnit);

        plantsToSpawn = (int)Math.Round(_plantsPerUnit.floatValue * terrainGenerator.terrainLength * terrainGenerator.terrainWidth);
        GUILayout.Label($"This will spawn {plantsToSpawn} plants");
        plantGenerator.amountOfPlantsToSpawn = plantsToSpawn;

        EditorGUILayout.PropertyField(_plantsPerFrame);


        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate plants"))
        {
            //Old logic ->
            //plantGenerator.StartCoroutine(plantGenerator.spawnPlantsInXZRange());

            //New logic ->
            plantGenerator.StartCoroutine(plantGenerator.SpawnPlantsOnChunks(terrainGenerator, _plantsPerFrame.intValue));
        }

        if (GUILayout.Button("Delete plants"))
        {
            plantGenerator.deleteAllPlants(terrainGenerator);
            EditorSceneManager.MarkAllScenesDirty();
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }

        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
