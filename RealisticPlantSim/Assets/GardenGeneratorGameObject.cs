using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using Unity.Transforms;

public class GardenGeneratorGameObject : MonoBehaviour
{
    public GameObject objectPrefab;
    public float spawnRadius;
    public uint objectCount;
    public uint spawnRate;

    uint spawnedObjects;

    // Start is called before the first frame update
    void Start()
    {
        AddObjects(objectCount);
        spawnedObjects = objectCount;
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            AddObjects(spawnRate);
            spawnedObjects += spawnRate;

            Debug.Log("Total objects: " + spawnedObjects);
        }
    }

    void AddObjects(uint amount)
    {
        for (int i = 0; i < (int)amount; i++)
        {
            Vector2 dir = Random.insideUnitCircle * spawnRadius;
            Vector2 rotationFwd = Random.insideUnitCircle;

            Instantiate(objectPrefab, new Vector3(dir.x, 0f, dir.y) + transform.position, Quaternion.LookRotation(new Vector3(rotationFwd.x, 0f, rotationFwd.y), Vector3.up), transform);
        }
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
