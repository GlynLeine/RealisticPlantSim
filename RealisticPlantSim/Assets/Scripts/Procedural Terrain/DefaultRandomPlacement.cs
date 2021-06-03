using UnityEngine;

public class DefaultRandomPlacement : AbstractPlacementStrategy
{
    public override void OnGeneratorFinish(PlantGenerator plantGenerator)
    {
        Debug.Log("Stopping generator");
    }

    public override void OnGeneratorStart(PlantGenerator plantGenerator)
    {
        Debug.Log("Starting generator");
    }

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
