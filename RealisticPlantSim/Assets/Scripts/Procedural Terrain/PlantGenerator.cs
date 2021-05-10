using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoudiniEngineUnity;
using UnityEditor;
using Unity.Jobs;
using Unity.Collections;

public class PlantGenerator : MonoBehaviour
{
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

    public int currentPlant = 0;
    public bool generatingPlants = false;


    public static PlantGenerator instance;
    private void Awake()
    {
        instance = this;
    }

    [System.Serializable]
    public class PlantSpawnSettings
    {
        public GameObject mainHoudiniPlant;
        public AbstractRandomPlacement randomPlacement;
        public List<PlantVariationSetting> randomizers;
    }

    [System.Serializable]
    public class PlantVariationSetting
    {
        public PlantVariationTypes type;
        public string houdiniKey;
        public float minValue;
        public float maxValue;

    }

    public enum PlantVariationTypes
    {
        Int,
        Float,
    }

    [SerializeField]
    public List<PlantSpawnSettings> plantSpawnSettings = new List<PlantSpawnSettings>();

    public IEnumerator spawnPlantsInXZRange()
    {
        generatingPlants = true;
        currentPlant = 0;
        Transform plantsHolder = transform.Find("Plants");
        if (plantsHolder == null)
        {
            plantsHolder = new GameObject("Plants").transform;
            plantsHolder.SetParent(transform);
        }

        for (int i = 0; i < amountOfPlantsToSpawn; i++)
        {
            float randomXOffset = Random.Range(xMinVal, xMaxVal);
            float randomZOffset = Random.Range(zMinVal, zMaxVal);

            Vector3 position = new Vector3(transform.position.x + randomXOffset, transform.position.y, transform.position.z + randomZOffset);
            int randomPlantIndex = Random.Range(0, plantSpawnSettings.Count);

            PlantSpawnSettings spawnSettings = plantSpawnSettings[randomPlantIndex];

            if (spawnSettings.mainHoudiniPlant == null)
            {
                Debug.LogError("Error creating plant: No main houdini plant found");
                yield break;
            }

            GameObject newPlant = generateNewPlant(spawnSettings);
            newPlant.transform.SetParent(plantsHolder);
            newPlant.transform.position = position;
            currentPlant++;
            yield return newPlant;
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
        generatingPlants = false;
        Debug.Log("[PlantGenerator] Plant generator finished!");
    }

    public void deleteAllPlants()
    {
        Transform plantsHolder = transform.Find("Plants");
        if (plantsHolder == null)
        {
            plantsHolder = new GameObject("Plants").transform;
            plantsHolder.SetParent(transform);
        }
        int childCount = plantsHolder.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject child = plantsHolder.GetChild(0).gameObject;
            DestroyImmediate(child);
        }
    }

    private GameObject generateNewPlant(PlantSpawnSettings spawnSettings)
    {
        StartCoroutine(randomizeHoudiniVars(spawnSettings));

        GameObject newPlant = new GameObject("Plant"); // TODO: change name


        //I = 1 because we skip the HDA_Data GameObject
        for(int i = 1; i < spawnSettings.mainHoudiniPlant.transform.childCount; i++)
        {
            GameObject plantMesh = Instantiate(spawnSettings.mainHoudiniPlant.transform.GetChild(i).gameObject);
            plantMesh.transform.SetParent(newPlant.transform);
        }
        return newPlant;
    }

    private IEnumerator randomizeHoudiniVars(PlantSpawnSettings spawnSettings)
    {
        HEU_HoudiniAssetRoot assetRoot = spawnSettings.mainHoudiniPlant.GetComponent<HEU_HoudiniAssetRoot>();

        //HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, "generations", Random.Range(4f, 10f));
        //HEU_ParameterUtility.SetInt(assetRoot._houdiniAsset, "randomseed", Random.Range(0, 1000));
        foreach(PlantVariationSetting variation in spawnSettings.randomizers)
        {
            switch(variation.type)
            {
                case PlantVariationTypes.Int:
                    HEU_ParameterUtility.SetInt(assetRoot._houdiniAsset, variation.houdiniKey, Random.Range((int)variation.minValue, (int)variation.maxValue));
                    break;
                case PlantVariationTypes.Float:
                    HEU_ParameterUtility.SetFloat(assetRoot._houdiniAsset, variation.houdiniKey, Random.Range(variation.minValue, variation.maxValue));
                    break;
            }
        }
        assetRoot._houdiniAsset.RequestCook(bCheckParametersChanged: true, bAsync: false, bSkipCookCheck: true, bUploadParameters: true);
        yield return null;
    }
}

