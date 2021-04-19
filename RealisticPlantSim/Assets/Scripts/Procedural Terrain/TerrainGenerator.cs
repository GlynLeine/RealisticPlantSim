using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private void Awake()
    {
        instance = this;
    }
    public static TerrainGenerator instance;

    public int chunkOffset = 2;
    public int textureWidth = 256;
    public int textureHeight = 256;
    public Material terrainMaterial;
    public int terrainSize = 10;

    public bool cacheGeneratedChunks = false;

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

    public Transform robotTransform;

    [SerializeField]
    public List<TerrainChunk> loadedChunks = new List<TerrainChunk>();

    public bool updateEveryFrame = false;

    public bool spawnPlants = true;
    private void Update()
    {
        //remove all caches causeor noise changes so our maps change aswell.
        if (updateEveryFrame)
        {
            foreach(TerrainChunk chunk in loadedChunks)
            {
                Destroy(chunk.chunkObject);
            }
            loadedChunks = new List<TerrainChunk>();
        }

        List<TerrainChunk> currentChunks = new List<TerrainChunk>();

        Vector2 robotGridPosition = new Vector2(Mathf.RoundToInt(robotTransform.position.x)/ terrainSize, Mathf.RoundToInt(robotTransform.position.z)/ terrainSize);
        for(int x = ((int)robotGridPosition.x - (int)chunkOffset); x < ((int)robotGridPosition.x + (int)chunkOffset); x++)
        {
            for (int y = ((int)robotGridPosition.y - (int)chunkOffset); y < ((int)robotGridPosition.y + (int)chunkOffset); y++)
            {

                TerrainChunk chunk = loadedChunks.Find(a => a.gridXpos/ terrainSize == x && a.gridYPos/ terrainSize == y);
                if (chunk != null)
                {
                    chunk.SetActive(true);
                    currentChunks.Add(chunk);
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(x* terrainSize, y* terrainSize);
                    newChunk.chunkObject.transform.SetParent(this.transform);
                    loadedChunks.Add(newChunk);
                    currentChunks.Add(newChunk);
                    if(spawnPlants)
                    {
                        if(PlantGenerator.instance != null && !PlantGenerator.instance.editorTimeGeneration)
                        {
                            PlantGenerator.instance.spawnPlantsOnChunk(newChunk);
                        }

                    }
                }
            }
        }

        foreach(TerrainChunk chunk in loadedChunks)
        {
            if (!currentChunks.Contains(chunk))
            {
                if (!cacheGeneratedChunks)
                {
                    DestroyImmediate(chunk.chunkObject);
                    loadedChunks.Remove(chunk);
                }
                else
                {
                    chunk.SetActive(false);
                }
            }
        }
    }
}

public class TerrainChunk
{
    public Texture2D heightMap;
    public int gridXpos;
    public int gridYPos;
    public GameObject chunkObject;

    public TerrainChunk(int x, int y)
    {
        this.gridXpos = x;
        this.gridYPos = y;
        this.heightMap = CreateHeightmap(x/ TerrainGenerator.instance.terrainSize, y/ TerrainGenerator.instance.terrainSize);
        chunkObject = createTerrain();
        this.SetActive(true);
    }

    public void SetActive(bool value)
    {
        chunkObject.SetActive(value);
    }

    public GameObject createTerrain()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Chunk: " + gridXpos + " : " + gridYPos;
        plane.transform.localScale = new Vector3(TerrainGenerator.instance.terrainSize*0.1f, 1, TerrainGenerator.instance.terrainSize*0.1f);
        plane.transform.position = new Vector3(gridXpos, 0, gridYPos);

        Material chunkMaterial = new Material(TerrainGenerator.instance.terrainMaterial);
        //chunkMaterial.SetTexture("_BaseColorMap", this.heightMap);
        chunkMaterial.SetTexture("_HeightMap", this.heightMap);


        plane.GetComponent<Renderer>().material = chunkMaterial;

        return plane;
    }

    /// <summary>
    /// creates a heightmap for the grid at the given x and y coordinates
    /// </summary>
    /// <param name="gridX"></param>
    /// <param name="gridY"></param>
    public Texture2D CreateHeightmap(int gridX, int gridY)
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


