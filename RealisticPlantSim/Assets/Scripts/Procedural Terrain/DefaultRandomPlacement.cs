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

    public override Vector3 RandomizePosition(PlantGenerator plantGenerator, float xmin, float xmax, float zmin, float zmax)
    {

        float randomXOffset = Random.Range(xmin, xmax);
        float randomZOffset = Random.Range(zmin, zmax);

        Vector3 position = new Vector3(randomXOffset, plantGenerator.transform.position.y, randomZOffset);
        Vector2 mappedPlantPosition = new Vector2(position.x, position.z);

        TerrainChunk terrainChunk = plantGenerator.gameObject.GetComponent<TerrainGenerator>().GetTerrainChunk(mappedPlantPosition);

        if (terrainChunk != null) {

            Vector2 terrainPosition = new Vector2(terrainChunk.gridXPos, terrainChunk.gridYPos);
            Vector2 relativePlantPosition = (mappedPlantPosition - terrainPosition) / terrainChunk.size;
            Vector2 plantUvPosition = new Vector2(0.5f, 0.5f) + relativePlantPosition;

            Color pixelColor = terrainChunk.heightMap.GetPixel((int)(plantUvPosition.x * terrainChunk.heightMap.width), (int)(plantUvPosition.y * terrainChunk.heightMap.height));

            float tesselationAmplitude = terrainChunk.chunkObject.GetComponent<Renderer>().sharedMaterial.GetFloat("_HeightTessAmplitude");
            float height = terrainChunk.chunkObject.transform.position.y + (pixelColor.r * tesselationAmplitude / 100f);
            position.y = height;
        }

        return position;
    }

    public override void OnInspectorGUI()
    {

    }

}
