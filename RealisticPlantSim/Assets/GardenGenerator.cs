using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using Unity.Transforms;

public class GardenGenerator : MonoBehaviour
{
    public GameObject entityPrefab;
    public float spawnRadius;
    public uint entityCount;
    public uint spawnRate;

    EntityManager manager;
    Entity srcEntity;

    uint spawnedEntities;

    // Start is called before the first frame update
    void Start()
    {
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        BlobAssetStore assetStore = new BlobAssetStore();
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, assetStore);
        srcEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(entityPrefab, settings);
        assetStore.Dispose();
        AddEntities(entityCount);
        spawnedEntities = entityCount;
    }

    private void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            AddEntities(spawnRate);
            spawnedEntities += spawnRate;

            Debug.Log("Total entities: " + spawnedEntities);
        }
    }

    void AddEntities(uint amount)
    {
        NativeArray<Entity> entities = manager.Instantiate(srcEntity, (int)amount, Allocator.Temp);

        Vector3 pos = transform.position;

        for (int i = 0; i < (int)amount; i++)
        {
            Vector2 dir = Random.insideUnitCircle * spawnRadius;
            Vector2 rotationFwd = Random.insideUnitCircle;
            manager.SetComponentData(entities[i], new Translation { Value = new float3(pos.x + dir.x, pos.y, pos.z + dir.y) });
            manager.SetComponentData(entities[i], new Rotation { Value = quaternion.LookRotation(new float3(rotationFwd.x, 0f, rotationFwd.y), new float3(0f, 1f, 0f)) });
        }

        entities.Dispose();

    }

    private void OnValidate()
    {
        spawnRadius = Mathf.Abs(spawnRadius);
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        Gizmos.matrix = new Matrix4x4(
            new Vector4(1f, 0f, 0f, 0f),
            new Vector4(0f, 0f, 0f, 0f),
            new Vector4(0f, 0f, 1f, 0f),
            new Vector4(pos.x, pos.y, pos.z, 1f));

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);

        Gizmos.DrawSphere(new Vector3(), spawnRadius);
    }
}
