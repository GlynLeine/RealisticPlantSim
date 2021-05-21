using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator instance;

    [Header("Terrain settings")]
    public int textureWidth = 256;
    public int textureHeight = 256;
    public Material terrainMaterial;
    public float terrainWidth, terrainLength;

    public float maximumChunkSize = 5f;

    [Header("noise settings")]
    [Range(0,100)]
    public float noiseScale = 1f;


    public int Seed;

    public float PerlinScale = 100f;
    public int PerlinOctaves = 5;
    public float persistence = 2f;
    public float lacunarity = 2f;
    public float PerlinBaseAmplitude = 1f;
    public float xOffset;
    public float yOffset;

    public bool includeSineWave = true;
    public float sinAmplitude = 0.2f;
    public float sinPeriod = .1f;
    public float perlinNoiseWeight = 0.5f;

    public void buildTerrain()
    {
        if(transform.Find("Terrain") == null)
        {
            GameObject terrain = new GameObject("Terrain");
            terrain.transform.SetParent(transform);
            float tileLengthLeft = terrainLength;
            int y = 0;
            while (tileLengthLeft > 0)
            {
                float tileLength = tileLengthLeft < maximumChunkSize ? tileLengthLeft : maximumChunkSize;

                float tileWidthLeft = terrainWidth;
                int x = 0;
                while (tileWidthLeft > 0)
                {
                    float tileWidth = tileWidthLeft < maximumChunkSize ? tileWidthLeft : maximumChunkSize;

                    TerrainChunk newChunk = new TerrainChunk(position: new Vector2(tileWidthLeft-(terrainWidth/2)+(0.5f*(maximumChunkSize-tileWidth)), tileLengthLeft-(terrainLength/2)+(0.5f*(maximumChunkSize-tileLength))), size: new Vector2(tileWidth, tileLength),index: new Vector2(x,y));
                    newChunk.chunkObject.transform.SetParent(terrain.transform);

                    x++;
                    tileWidthLeft -= maximumChunkSize;
                }
                y++;
                tileLengthLeft -= maximumChunkSize;
            }
            
        } else
        {
            deleteTerrain();
            buildTerrain();
        }

    }
    public void deleteTerrain()
    {
        Transform terrain = transform.Find("Terrain");
        if(terrain != null)
        {
            DestroyImmediate(terrain.gameObject);
        } else
        {
            Debug.LogError("[TerrainGenerator] There is no terrain to delete!");
        }
    }
}

public class TerrainChunk
{
    public Texture2D heightMap;
    public float gridXpos;
    public float gridYPos;
    public GameObject chunkObject;

    public TerrainChunk(Vector2 position, Vector2 size, Vector2 index)
    {

        this.gridXpos = position.x;
        this.gridYPos = position.y;
        this.heightMap = CreateHeightmap(index.x,index.y);
        chunkObject = createTerrain(size.x,size.y);
        this.SetActive(true);
    }

    public void SetActive(bool value)
    {
        chunkObject.SetActive(value);
    }

    public GameObject createTerrain(float width, float height)
    {
        GameObject plane = new GameObject("Chunk");
        plane.AddComponent<MeshFilter>().mesh = CreateTerrainMesh(width, height);
        plane.AddComponent<MeshRenderer>();
        plane.transform.position = new Vector3(gridXpos, 0, gridYPos);

        Material chunkMaterial = new Material(TerrainGenerator.instance.terrainMaterial);
        //chunkMaterial.SetTexture("_BaseColorMap", this.heightMap);
        chunkMaterial.SetTexture("_HeightMap", this.heightMap);


        plane.GetComponent<Renderer>().material = chunkMaterial;

        return plane;
    }

    public Mesh CreateTerrainMesh(float width, float height)
    {
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        Mesh m = new Mesh();
        m.name = "chunk mesh";
        m.vertices = new Vector3[] {
         new Vector3(-halfWidth, 0.01f,-halfHeight),
         new Vector3(halfWidth, 0.01f,-halfHeight),
         new Vector3(halfWidth, 0.01f, halfHeight),
         new Vector3(-halfWidth, 0.01f, halfHeight)
        };

        Vector2 uvScaled = new Vector2(width / TerrainGenerator.instance.maximumChunkSize, height / TerrainGenerator.instance.maximumChunkSize);

        m.uv = new Vector2[] {
         new Vector2 (1f-uvScaled.x, 0f),
         new Vector2 (1f, 0f),
         new Vector2(1f, uvScaled.y ),
         new Vector2 (1f-uvScaled.x, uvScaled.y)


         //new Vector2 (1f-uvScaled.x, 0f),
         //new Vector2 (uvScaled.x, 0f),
         //new Vector2(uvScaled.x, uvScaled.y ),
         //new Vector2 (1f-uvScaled.x, uvScaled.y)
        };
        m.triangles = new int[] { 2, 1, 0, 0, 3, 2 };
        m.RecalculateNormals();

        return m;
    }

    /// <summary>
    /// creates a heightmap for the grid at the given x and y coordinates
    /// </summary>
    /// <param name="gridX"></param>
    /// <param name="gridY"></param>
    public Texture2D CreateHeightmap(float gridX, float gridY)
    {
        Texture2D heightMapTexture = new Texture2D(TerrainGenerator.instance.textureWidth, TerrainGenerator.instance.textureHeight);
        Vector2[] octaveOffsets = new Vector2[TerrainGenerator.instance.PerlinOctaves];

        float halfWidth = TerrainGenerator.instance.textureWidth / 2;
        float halfHeight = TerrainGenerator.instance.textureHeight / 2;

        // Set min / max
        float maxNoiseHeight = float.MaxValue;
        float minNoiseHeight = float.MinValue;
        // Set Max Height for World
        float maxPossibleHeight = 0;
        float amplitude = TerrainGenerator.instance.PerlinBaseAmplitude;

        System.Random prng = new System.Random(TerrainGenerator.instance.Seed);
        float offsetX = prng.Next(-100000, 100000) - TerrainGenerator.instance.xOffset - (TerrainGenerator.instance.textureWidth * gridX);
        float offsetY = prng.Next(-100000, 100000) - TerrainGenerator.instance.yOffset - (TerrainGenerator.instance.textureHeight * gridY);

        //combine noises 
        for (int i = 0; i < TerrainGenerator.instance.PerlinOctaves; i++)
        {
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= TerrainGenerator.instance.persistence;
        }


        //generate map for our tesselation shader on our chunk
        for (int x = 0; x < TerrainGenerator.instance.textureWidth; x++)
        {
            for (int y = 0; y < TerrainGenerator.instance.textureHeight; y++)
            {
                amplitude = TerrainGenerator.instance.PerlinBaseAmplitude;
                float freq = 1;
                float noiseHeight = 0;

                //perlin mapfrom combined octaves
                for (int i = 0; i < TerrainGenerator.instance.PerlinOctaves; i++)
                {
                    float px = (float)(x - halfWidth + octaveOffsets[i].x) / TerrainGenerator.instance.PerlinScale * freq;
                    float py = (float)(y - halfHeight + octaveOffsets[i].y) / TerrainGenerator.instance.PerlinScale * freq;

                    float PerlinValue = Mathf.PerlinNoise(px, py) * 2 - 1;
                    noiseHeight += PerlinValue * amplitude;

                    amplitude *= TerrainGenerator.instance.persistence;
                    freq *= TerrainGenerator.instance.lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                // Normalize Sample to fit world Sample Height
                float normalizedHeight = (noiseHeight + 1) / (2f * maxPossibleHeight / 1.75f);
                float value = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);

                if (TerrainGenerator.instance.includeSineWave)
                {
                    //combining sine wave with perlin noise
                    float sin = TerrainGenerator.instance.sinAmplitude * Mathf.Sin(TerrainGenerator.instance.sinPeriod * (x + offsetX));
                    value = Mathf.Clamp(sin + (value * TerrainGenerator.instance.perlinNoiseWeight), 0, int.MaxValue);
                }

                Color color = new Color(value, value, value);
                heightMapTexture.SetPixel(x, y, color);
            }
        }
        heightMapTexture.wrapMode = TextureWrapMode.Clamp;
        heightMapTexture.Apply();
        return heightMapTexture;
    }
}


