using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoudiniEngineUnity;
using UnityEditor;

public class PlantGenerator : MonoBehaviour
{

    public static PlantGenerator instance;
    private void Awake()
    {
        instance = this;
    }

    [System.Serializable]
    public class PlantSpawnSettings
    {
        public Object plant;
        public float generations = 1;

    }

    public int plantsPerChunk = 15;

    [SerializeField]
    public List<PlantSpawnSettings> plantSpawnSettings = new List<PlantSpawnSettings>();

    public void spawnPlantsOnChunk(TerrainChunk chunk)
    {
        for (int i = 0; i < plantsPerChunk; i++)
        {
            float randomXOffset = Random.Range(-5f, 5f);
            float randomYOffset = Random.Range(-5f, 5f);

            Vector3 position = new Vector3(chunk.gridXpos + randomXOffset, 0, chunk.gridYPos + randomYOffset);
            int randomPlantIndex = Random.Range(0, plantSpawnSettings.Count);
            GameObject placedPlant = HEU_HAPIUtility.InstantiateHDA(AssetDatabase.GetAssetPath(plantSpawnSettings[randomPlantIndex].plant), position, HEU_SessionManager.GetOrCreateDefaultSession(), false);
            placedPlant.transform.SetParent(chunk.chunkObject.transform);
            if (placedPlant != null && placedPlant.GetComponent<HEU_HoudiniAssetRoot>() != null)
            {
                HEU_HoudiniAssetRoot assetRoot = placedPlant.GetComponent<HEU_HoudiniAssetRoot>();
                if (assetRoot != null)
                {
                    HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", Random.Range(5f,11f));
                    assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: true, bSkipCookCheck: true, bUploadParameters: true);

                    // assetRoot._houdiniAsset;
                }
            }
        }
    }
}

