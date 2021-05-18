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

    public override Vector3 RandomizePosition(PlantGenerator plantGenerator)
    {

        float randomXOffset = Random.Range(plantGenerator.xMinVal, plantGenerator.xMaxVal);
        float randomZOffset = Random.Range(plantGenerator.zMinVal, plantGenerator.zMaxVal);

        Vector3 position = new Vector3(plantGenerator.transform.position.x + randomXOffset, plantGenerator.transform.position.y, plantGenerator.transform.position.z + randomZOffset);

        return position;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Test"))
        {
            Debug.Log("Damn it works!");
        }
    }

}
