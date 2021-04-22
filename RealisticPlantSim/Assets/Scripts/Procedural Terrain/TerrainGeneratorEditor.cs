using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        if (GUILayout.Button("Generate terrain"))
        {
            TerrainGenerator terrainGenerator = (TerrainGenerator)target;
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.buildTerrain(new Vector2(0, 0), new Vector2(terrainGenerator.terrainWidth, terrainGenerator.terrainLength));

        }

        serializedObject.ApplyModifiedProperties();
    }
}
