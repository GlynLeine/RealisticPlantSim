using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Places plants on the terrain tops based on the sine function of the generated terrain.
/// Author: Robin Dittrich, Glyn Leine, Maurijn Besters
/// </summary>
public class FarmlandHeightBasedPlacementStrategy : AbstractPlacementStrategy
{

    static float period;
    static List<float> possibleXValues;
    static TerrainGenerator terrainGenerator;

    static float maxOffset = 0;

    /// <summary>
    /// OnGeneratorStart calculate the positions of the terrain tops based on the terraingenerator settings
    /// </summary>
    /// <param name="plantGenerator"></param>
    public override void OnGeneratorStart(PlantGenerator plantGenerator)
    {
        terrainGenerator = plantGenerator.gameObject.GetComponent<TerrainGenerator>();
        possibleXValues = new List<float>();


        //float period = sinePeriod * x;
        //float sine = (sin(period) * halfSineAmplitude) + halfSineAmplitude;

        period = (Mathf.PI * 2f) / terrainGenerator.sinPeriod;

        Vector3 fieldCenter = plantGenerator.gameObject.transform.position;

        float halfWidth = terrainGenerator.terrainWidth / 2f;
        float minOffset = Mathf.Repeat(halfWidth - (period * 0.75f), period);
        float minXValue = (fieldCenter.x - terrainGenerator.terrainWidth / 2) + minOffset;
        float maxXValue = (fieldCenter.x + terrainGenerator.terrainWidth / 2);



        string log = "";

        for(float i = minXValue; i <= maxXValue; i+= period)
        {
            possibleXValues.Add(i);
            log += " " + i.ToString();
        }

        Debug.Log(log);

    }

    /// <summary>
    /// OnGeneratorFinish reset the positions that plants can be placed on
    /// </summary>
    /// <param name="plantGenerator"></param>
    public override void OnGeneratorFinish(PlantGenerator plantGenerator)
    {
        possibleXValues = null;
    }

    /// <summary>
    /// Inspector spot for a maxRowOffset where a plant can be a little offset on a row top.
    /// TODO: fix the variable storage problem here
    /// </summary>
    /// <param name="plantGenerator"></param>
    public override void OnInspectorGUI(PlantGenerator plantGenerator)
    {
        //float maxRowOffset = plantGenerator.getVarFromPlacementVarStorage<float>("maxRowOffset");

        //float offset = EditorGUILayout.FloatField(new GUIContent("Maximum offset"), maxRowOffset);

        //if (offset != maxRowOffset)
        //{
        //    plantGenerator.placementStrategyVars["maxRowOffset"] = offset;
        //}

    }

    
    public override Vector3 RandomizePosition(PlantGenerator plantGenerator, TerrainChunk chunk, float xmin, float xmax, float zmin, float zmax)
    {
        List<float> possibleXValuesForThisChunk = possibleXValues.Where(x => x >= (chunk.gridXPos - chunk.size.x / 2f) && x <= (chunk.gridXPos + chunk.size.x/2f)).ToList();
        int randomRowIndex = Random.Range(0, possibleXValuesForThisChunk.Count);
        float randomZOffset = Random.Range(zmin, zmax);

        Vector3 position = new Vector3(possibleXValuesForThisChunk[randomRowIndex], plantGenerator.transform.position.y, randomZOffset);
        Vector2 mappedPlantPosition = new Vector2(position.x, position.z);

        position.y = chunk.GetHeightFromPosition(mappedPlantPosition);
        

        return position;
    }
}
