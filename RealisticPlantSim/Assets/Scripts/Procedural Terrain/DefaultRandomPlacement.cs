using UnityEngine;

/// <summary>
/// Default random placement that randomly places plants within minimum and maximum bounds
/// Author: Robin Dittrich
/// </summary>
public class DefaultRandomPlacement : AbstractPlacementStrategy
{

    public override void OnGeneratorFinish(PlantGenerator plantGenerator)
    {
    }

    public override void OnGeneratorStart(PlantGenerator plantGenerator)
    {
    }

    /// <summary>
    /// Randomizes position of a plant on the targeted chunk.
    /// </summary>
    /// <param name="plantGenerator"></param>
    /// <param name="chunk"></param>
    /// <param name="xmin"></param>
    /// <param name="xmax"></param>
    /// <param name="zmin"></param>
    /// <param name="zmax"></param>
    /// <returns>Vector3 with randomized position</returns>
    public override Vector3 RandomizePosition(PlantGenerator plantGenerator, TerrainChunk chunk, float xmin, float xmax, float zmin, float zmax)
    {

        float randomXOffset = Random.Range(xmin, xmax);
        float randomZOffset = Random.Range(zmin, zmax);

        Vector3 position = new Vector3(randomXOffset, plantGenerator.transform.position.y, randomZOffset);
        Vector2 mappedPlantPosition = new Vector2(position.x, position.z);

        position.y = chunk.GetHeightFromPosition(mappedPlantPosition);

        return position;
    }

    public override void OnInspectorGUI(PlantGenerator plantGenerator)
    {
    }

}
