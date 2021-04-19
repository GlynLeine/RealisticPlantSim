using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoudiniEngineUnity;
using UnityEditor;

public class PlantGenerator : MonoBehaviour
{
    [SerializeField]
    public bool editorTimeGeneration = true;

    [SerializeField]
    public float xMinVal = -10f;
    [SerializeField]
    public float zMinVal = -10f;
    [SerializeField]
    public float xMaxVal = 10f;
    [SerializeField]
    public float zMaxVal = 10f;

    [SerializeField]
    public int amountOfPlantsToSpawn = 100;


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

    public IEnumerator spawnPlantsInXZRange()
    {
        for(int i = 0; i < amountOfPlantsToSpawn; i++)
        {
            float randomXOffset = Random.Range(xMinVal, xMaxVal);
            float randomZOffset = Random.Range(zMinVal, zMaxVal);

            Vector3 position = new Vector3(transform.position.x + randomXOffset, transform.position.y, transform.position.z + randomZOffset);
            int randomPlantIndex = Random.Range(0, plantSpawnSettings.Count);

            PlantSpawnSettings spawnSettings = plantSpawnSettings[randomPlantIndex];

            if (spawnSettings.mainHoudiniPlant == null)
            {
                spawnSettings.mainHoudiniPlant = createHoudiniPlant(spawnSettings);
                if (spawnSettings.mainHoudiniPlant == null) yield break; //Return because something went horribly wrong.
            }

            GameObject newPlant = generateNewPlantVariantEditor(spawnSettings);
            yield return newPlant;
            newPlant.transform.SetParent(transform);
            newPlant.transform.position = position;
            Debug.Log($"Spawned plant {i + 1}/{amountOfPlantsToSpawn}");
        }

    }

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
        }
    }


    private GameObject generateNewPlantVariant(PlantSpawnSettings spawnSettings)
    {
        HEU_HoudiniAssetRoot assetRoot = spawnSettings.mainHoudiniPlant.GetComponent<HEU_HoudiniAssetRoot>();
        if(assetRoot != null)
        {
            StartCoroutine(randomizeHoudiniVars(assetRoot));
        }

        GameObject clonedPlant = Instantiate(spawnSettings.mainHoudiniPlant);
        return clonedPlant;
    }

    private GameObject generateNewPlantVariantEditor(PlantSpawnSettings spawnSettings)
    {
        HEU_HoudiniAssetRoot assetRoot = spawnSettings.mainHoudiniPlant.GetComponent<HEU_HoudiniAssetRoot>();
        if (assetRoot != null)
        {
            StartCoroutine(randomizeHoudiniVars(assetRoot));
        }

        GameObject newPlant = new GameObject("Plant"); // TODO: change name


        //I = 1 because we skip the HDA_Data GameObject
        for(int i = 1; i < spawnSettings.mainHoudiniPlant.transform.childCount; i++)
        {
            GameObject plantMesh = Instantiate(spawnSettings.mainHoudiniPlant.transform.GetChild(i).gameObject);
            plantMesh.transform.SetParent(newPlant.transform);
        }
        return newPlant;
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

    private IEnumerator randomizeHoudiniVars(HEU_HoudiniAssetRoot assetRoot)
    {
        HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", Random.Range(4f, 10f));
        HEU_ParameterUtility.SetInt(assetRoot._houdiniAsset, "randomseed", Random.Range(0, 1000));
        assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: true, bUploadParameters: true);
        yield return null;
    }
}

