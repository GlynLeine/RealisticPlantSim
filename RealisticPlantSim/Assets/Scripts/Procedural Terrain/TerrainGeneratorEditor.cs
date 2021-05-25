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
            terrainGenerator.buildTerrain();

        }

        if (GUILayout.Button("Delete terrain"))
        {
            TerrainGenerator terrainGenerator = (TerrainGenerator)target;
            TerrainGenerator.instance = terrainGenerator;
            terrainGenerator.deleteTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
