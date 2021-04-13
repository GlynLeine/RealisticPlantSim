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
        public GameObject mainHoudiniPlant;

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

            PlantSpawnSettings spawnSettings = plantSpawnSettings[randomPlantIndex];

            if(spawnSettings.mainHoudiniPlant == null)
            {
                spawnSettings.mainHoudiniPlant = createHoudiniPlant(spawnSettings);
                if (spawnSettings.mainHoudiniPlant == null) return; //Return because something went horribly wrong.
            }

            GameObject newPlant = generateNewPlantVariant(spawnSettings);

            newPlant.transform.SetParent(chunk.chunkObject.transform);
            newPlant.transform.position = position;


            //GameObject placedPlant = HEU_HAPIUtility.InstantiateHDA(AssetDatabase.GetAssetPath(plantSpawnSettings[randomPlantIndex].plant), position, HEU_SessionManager.GetOrCreateDefaultSession(), false);
            //placedPlant.transform.SetParent(chunk.chunkObject.transform);
            //if (placedPlant != null && placedPlant.GetComponent<HEU_HoudiniAssetRoot>() != null)
            //{
            //    HEU_HoudiniAssetRoot assetRoot = placedPlant.GetComponent<HEU_HoudiniAssetRoot>();
            //    if (assetRoot != null)
            //    {
            //        HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", Random.Range(5f,11f));
            //        assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: true, bSkipCookCheck: true, bUploadParameters: true);

            //        // assetRoot._houdiniAsset;
            //    }
            //}
        }
    }


    private GameObject generateNewPlantVariant(PlantSpawnSettings spawnSettings)
    {
        HEU_HoudiniAssetRoot assetRoot = spawnSettings.mainHoudiniPlant.GetComponent<HEU_HoudiniAssetRoot>();
        if(assetRoot != null)
        {
            randomizeHoudiniVars(assetRoot);
        }

        GameObject clonedPlant = Instantiate(spawnSettings.mainHoudiniPlant);
        Destroy(clonedPlant.GetComponent<HEU_HoudiniAssetRoot>());
        Destroy(clonedPlant.transform.Find("HDA_Data"));
        return clonedPlant;
    }

    private GameObject createHoudiniPlant(PlantSpawnSettings spawnSettings)
    {
        GameObject placedPlant = HEU_HAPIUtility.InstantiateHDA(AssetDatabase.GetAssetPath(spawnSettings.plant), new Vector3(0, 500, 0), HEU_SessionManager.GetOrCreateDefaultSession(), false);
        if (placedPlant != null && placedPlant.GetComponent<HEU_HoudiniAssetRoot>() != null)
        {
            HEU_HoudiniAssetRoot assetRoot = placedPlant.GetComponent<HEU_HoudiniAssetRoot>();
            if (assetRoot != null)
            {
                HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", 5);
                assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: true, bUploadParameters: true);

                // assetRoot._houdiniAsset;
                return placedPlant;
            }
        }

        return null;

    }

    private void randomizeHoudiniVars(HEU_HoudiniAssetRoot assetRoot)
    {
        HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", Random.Range(4f, 10f));
        assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: true, bUploadParameters: true);
    }
}

