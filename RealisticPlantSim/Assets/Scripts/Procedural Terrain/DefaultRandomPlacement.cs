using UnityEngine;

public class DefaultRandomPlacement : AbstractRandomPlacement
{
    public override Vector3 randomizePosition(PlantGenerator plantGenerator)
    {

        float randomXOffset = Random.Range(plantGenerator.xMinVal, plantGenerator.xMaxVal);
        float randomZOffset = Random.Range(plantGenerator.zMinVal, plantGenerator.zMaxVal);

        Vector3 position = new Vector3(plantGenerator.transform.position.x + randomXOffset, plantGenerator.transform.position.y, plantGenerator.transform.position.z + randomZOffset);

        return position;
    }
}
