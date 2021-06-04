using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoudiniEngineUnity;
using UnityEditor;
using System;
using Random = UnityEngine.Random;
using System.Linq;

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

    [SerializeField]
    public SerializableDictionary<string, object> placementStrategyVars = new SerializableDictionary<string, object>();

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
        public bool enabled = true;
        public GameObject mainHoudiniPlant;
        public MonoScript placementStrategy;
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

    public IEnumerator SpawnPlantsOnChunks(TerrainGenerator terrainGenerator, int plantsPerFrame)
    {
        generatingPlants = true;
        currentPlant = 0;
        int amountPerChunk = amountOfPlantsToSpawn / terrainGenerator.chunks.Count;

        List<PlantSpawnSettings> enabledPlantSpawnSettings = plantSpawnSettings.Where(pss => pss.enabled).ToList();

        //First loop through all the PlantSpawnSettings to call the onGeneratorStart function.

        foreach (PlantSpawnSettings spawnSetting in enabledPlantSpawnSettings)
        {
            AbstractPlacementStrategy placementStrategy;

            if (spawnSetting.placementStrategy.GetClass().IsSubclassOf(typeof(AbstractPlacementStrategy)))
            {
                placementStrategy = Activator.CreateInstance(spawnSetting.placementStrategy.GetClass()) as AbstractPlacementStrategy;
            }
            else
            {
                throw new Exception($"[PlantGenerator] The Random Placement script on plant #{plantSpawnSettings.IndexOf(spawnSetting)} does not extend from AbstractPlacementStrategy!");
            }
            placementStrategy.OnGeneratorStart(this);
        }

        if(terrainGenerator.chunks == null)
        {
            throw new Exception("[PlantGenerator] TerrainGenerator has a chunks list that is NULL!");
        }

        if(terrainGenerator.chunks.Count == 0)
        {
            throw new Exception("[PlantGenerator] TerrainGenerator has no chunks!");
        }

        //Turning off chunks for performance
        foreach (TerrainChunk chunk in terrainGenerator.chunks)
        {
            chunk.SetActive(false);
        }


        List<TerrainChunk> chunksToGenerateOn = terrainGenerator.chunks.ToList();

        while(chunksToGenerateOn.Count > 0)
        {
            List<Coroutine> plantSpawnerCoroutines = new List<Coroutine>();

            foreach (TerrainChunk chunk in chunksToGenerateOn.RemoveAndGet(0, plantsPerFrame))
            {
                plantSpawnerCoroutines.Add(StartCoroutine(SpawnPlantsOnChunk(chunk, enabledPlantSpawnSettings, amountPerChunk)));
            }

            foreach (Coroutine coroutine in plantSpawnerCoroutines)
            {
                yield return coroutine;
            }
        }




        //Turning chunks back on
        foreach (TerrainChunk chunk in terrainGenerator.chunks)
        {
            chunk.SetActive(true);
        }

        //Call OnGeneratorFinish on every random placement script

        foreach (PlantSpawnSettings spawnSetting in enabledPlantSpawnSettings)
        {
            AbstractPlacementStrategy placementStrategy;

            if (spawnSetting.placementStrategy.GetClass().IsSubclassOf(typeof(AbstractPlacementStrategy)))
            {
                placementStrategy = Activator.CreateInstance(spawnSetting.placementStrategy.GetClass()) as AbstractPlacementStrategy;
            }
            else
            {
                throw new Exception($"[PlantGenerator] The Random Placement script on plant #{plantSpawnSettings.IndexOf(spawnSetting)} does not extend from AbstractPlacementStrategy!");
            }
            placementStrategy.OnGeneratorFinish(this);
        }

        Debug.Log("[PlantGenerator] Plant generator finished!");
        generatingPlants = false;

    }

    public IEnumerator SpawnPlantsOnChunk(TerrainChunk chunk, List<PlantSpawnSettings> plantSpawnSettings, int amountOfPlants)
    {

        for (int i = 0; i < amountOfPlants; i++)
        {
            int randomPlantIndex = Random.Range(0, plantSpawnSettings.Count);

            PlantSpawnSettings spawnSettings = plantSpawnSettings[randomPlantIndex];

            AbstractPlacementStrategy placementStrategy;

            if (spawnSettings.placementStrategy.GetClass().IsSubclassOf(typeof(AbstractPlacementStrategy)))
            {
                placementStrategy = Activator.CreateInstance(spawnSettings.placementStrategy.GetClass()) as AbstractPlacementStrategy;
            }
            else
            {
                throw new Exception($"[PlantGenerator] The Random Placement script on plant #{randomPlantIndex} does not extend from AbstractPlacementStrategy!");
            }
            float chunkX = chunk.chunkObject.transform.position.x;
            float chunkZ = chunk.chunkObject.transform.position.z;
            float chunkSizeXHalf = chunk.size.x/2;
            float chunkSizeZHalf = chunk.size.y/2;
            Vector3 position = placementStrategy.RandomizePosition(this, chunk, chunkX-chunkSizeXHalf, chunkX+ chunkSizeXHalf, chunkZ-chunkSizeZHalf, chunkZ+chunkSizeZHalf);

            if (spawnSettings.mainHoudiniPlant == null)
            {
                Debug.LogError("Error creating plant: No main houdini plant found");
                yield break;
            }

            GameObject newPlant = generateNewPlant(spawnSettings);
            newPlant.transform.SetParent(chunk.chunkObject.transform);
            newPlant.transform.position = position;
            newPlant.isStatic = true;
            currentPlant++;
            yield return newPlant;
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

    }

    public void deleteAllPlants(TerrainGenerator terrainGenerator)
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

        foreach (TerrainChunk chunk in terrainGenerator.chunks)
        {
            int plantCount = chunk.chunkObject.transform.childCount;
            for (int i = 0; i < plantCount; i++)
            {
                GameObject child = chunk.chunkObject.transform.GetChild(0).gameObject;
                DestroyImmediate(child);
            }
        }
    }

    private GameObject generateNewPlant(PlantSpawnSettings spawnSettings)
    {
        StartCoroutine(randomizeHoudiniVars(spawnSettings));

        GameObject newPlant = new GameObject("Plant"); // TODO: change name
        newPlant.transform.localScale = spawnSettings.mainHoudiniPlant.transform.localScale;

        //I = 1 because we skip the HDA_Data GameObject
        for(int i = 1; i < spawnSettings.mainHoudiniPlant.transform.childCount; i++)
        {
            GameObject plantPart = Instantiate(spawnSettings.mainHoudiniPlant.transform.GetChild(i).gameObject);
            plantPart.transform.SetParent(newPlant.transform);
            plantPart.transform.localScale = spawnSettings.mainHoudiniPlant.transform.GetChild(i).localScale;

        }
        return newPlant;
    }

    private IEnumerator randomizeHoudiniVars(PlantSpawnSettings spawnSettings)
    {
        HEU_HoudiniAssetRoot assetRoot = spawnSettings.mainHoudiniPlant.GetComponent<HEU_HoudiniAssetRoot>();
        foreach (PlantVariationSetting variation in spawnSettings.randomizers)
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

    public T getVarFromPlacementVarStorage<T>(string key)
    {
        if(!placementStrategyVars.ContainsKey(key))
        {
            return default(T);
        }

        object value = placementStrategyVars[key];
        if(value.GetType() is T)
        {
            return (T)value;
        } else
        {
            return default(T);
        }
    }
}

public static class Extensions
{
    public static IList<T> RemoveAndGet<T>(this IList<T> list, int index, int amount)
    {
        lock (list)
        {
            List<T> itemsTaken = new List<T>();
            for(int i = 0; i < amount; i++)
            {
                int actualIndex = index + i;
                if(list.ElementAtOrDefault(actualIndex) != null)
                {
                    itemsTaken.Add(list[actualIndex]);
                    list.RemoveAt(actualIndex);

                }
            }
            return itemsTaken;
        }
    }
}

//[Serializable]
//public class PlacementStrategyValueStore
//{
//    public string paramName;

//    public PlacementStrategyValueStore(string )
//    {
//        this.paramName = paramName;

//    }
//}
