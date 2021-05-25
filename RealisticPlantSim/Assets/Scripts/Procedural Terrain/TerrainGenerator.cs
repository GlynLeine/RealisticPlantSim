using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator instance;

    [Header("Terrain settings")]
    public int textureWidth = 256;
    public int textureHeight = 256;
    public Material terrainMaterial;
    public Texture2D baseHeightTexture;
    public float terrainWidth, terrainLength;

    public float maximumChunkSize = 5f;

    [Header("noise settings")]
    [Range(0, 100)]
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
        if (transform.Find("Terrain") == null)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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

                    TerrainChunk newChunk = new TerrainChunk(position: new Vector2(tileWidthLeft - (terrainWidth / 2) + (0.5f * (maximumChunkSize - tileWidth)), tileLengthLeft - (terrainLength / 2) + (0.5f * (maximumChunkSize - tileLength))), size: new Vector2(tileWidth, tileLength), index: new Vector2(x, y));
                    newChunk.chunkObject.transform.SetParent(terrain.transform);

                    x++;
                    tileWidthLeft -= maximumChunkSize;
                }
                y++;
                tileLengthLeft -= maximumChunkSize;
            }

            Debug.Log("Total: " + stopwatch.Elapsed.TotalSeconds.ToString());
        }
        else
        {
            deleteTerrain();
            buildTerrain();
        }

    }
    public void deleteTerrain()
    {
        Transform terrain = transform.Find("Terrain");
        if (terrain != null)
        {
            DestroyImmediate(terrain.gameObject);
        }
        else
        {
            Debug.LogError("[TerrainGenerator] There is no terrain to delete!");
        }
    }
}

public class TerrainChunk
{
    public Texture2D heightMap;
    public Texture2D normalMap;
    public float gridXpos;
    public float gridYPos;
    public GameObject chunkObject;

    public TerrainChunk(Vector2 position, Vector2 size, Vector2 index)
    {

        this.gridXpos = position.x;
        this.gridYPos = position.y;
        this.heightMap = CreateHeightmap(index.x, index.y);
        this.normalMap = this.heightMap.CreateNormal(10f);
        chunkObject = createTerrain(size.x, size.y);
        this.SetActive(true);
    }

    public void SetActive(bool value)
    {
        chunkObject.SetActive(value);
    }

    public GameObject createTerrain(float width, float height)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        GameObject plane = new GameObject("Chunk");
        plane.AddComponent<MeshFilter>().mesh = CreateTerrainMesh(width, height);
        plane.AddComponent<MeshRenderer>();
        plane.transform.position = new Vector3(gridXpos, 0, gridYPos);

        Material chunkMaterial = new Material(TerrainGenerator.instance.terrainMaterial);
        //chunkMaterial.SetTexture("_BaseColorMap", this.heightMap);
        chunkMaterial.SetTexture("_HeightMap", this.heightMap);
        chunkMaterial.SetTexture("_NormalMap", this.normalMap);


        plane.GetComponent<Renderer>().material = chunkMaterial;

        Debug.Log("terrain: " + stopwatch.Elapsed.TotalSeconds.ToString());
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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

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

        TextureScale.Bilinear(heightMapTexture, TerrainGenerator.instance.baseHeightTexture.width, TerrainGenerator.instance.baseHeightTexture.height);
        heightMapTexture.Apply();
        double time;
        Texture2D blended = heightMapTexture.Blend(TerrainGenerator.instance.baseHeightTexture, .5f, out time);

        blended.wrapMode = TextureWrapMode.Clamp;
        blended.Apply();

        Debug.Log("height tex: " + (stopwatch.Elapsed.TotalSeconds - time).ToString());
        return blended;
    }


}

public static class ColorUtils
{
    struct Vector3Uint
    {
        public uint x, y, z;
    };

    static ComputeShader m_blendShader;
    static int m_blendKernel;
    static Vector3Uint m_blendGlobal;

    public static Texture2D Blend(this Texture2D texture, Texture2D otherTexture, float amount, out double time)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!m_blendShader)
        {
            m_blendShader = Resources.Load<ComputeShader>("Compute/Blend");
            if (!m_blendShader)
            {
                Debug.LogError("Could not load blend compute shader.");
                time = stopwatch.Elapsed.TotalSeconds;
                return null;
            }

            m_blendKernel = m_blendShader.FindKernel("CSBlend");
            m_blendShader.GetKernelThreadGroupSizes(m_blendKernel, out m_blendGlobal.x,  out m_blendGlobal.y, out m_blendGlobal.z);
        }

        m_blendShader.SetFloat("strength", amount);

        Texture2D result = new Texture2D(texture.width, texture.height, texture.format, false);

        m_blendShader.SetTexture(m_blendKernel, "target", result);
        m_blendShader.SetTexture(m_blendKernel, "sourceA", texture);
        m_blendShader.SetTexture(m_blendKernel, "sourceB", otherTexture);

        Vector3Int dispatchGroupSize = new Vector3Int(texture.width / (int)m_blendGlobal.x, texture.height / (int)m_blendGlobal.y, 1);
        m_blendShader.Dispatch(m_blendKernel, dispatchGroupSize.x, dispatchGroupSize.y, dispatchGroupSize.z);

        result.filterMode = texture.filterMode;
        result.anisoLevel = texture.anisoLevel;
        result.wrapModeU = texture.wrapModeU;
        result.wrapModeV = texture.wrapModeV;
        result.wrapModeW = texture.wrapModeW;

        result.Apply(texture.mipmapCount > 1, !texture.isReadable);

        time = stopwatch.Elapsed.TotalSeconds;
        Debug.Log("blend tex: " + time.ToString());
        return result;
    }

    public static Color Blend(this Color color, Color backColor, double amount)
    {
        float r = (float)((color.r * amount) + backColor.r * (1 - amount));
        float g = (float)((color.g * amount) + backColor.g * (1 - amount));
        float b = (float)((color.b * amount) + backColor.b * (1 - amount));
        return new Color(r, g, b, 1.0f);
    }

    public static Texture2D CreateNormal(this Texture2D source, float strength)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        strength = Mathf.Clamp(strength, 0.0F, 10.0F);

        Texture2D result;

        float xLeft;
        float xRight;
        float yUp;
        float yDown;
        float yDelta;
        float xDelta;

        result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

        for (int by = 0; by < result.height + 1; by++)
        {
            for (int bx = 0; bx < result.width + 1; bx++)
            {
                xLeft = source.GetPixel(bx - 1, by).grayscale * strength;
                xRight = source.GetPixel(bx + 1, by).grayscale * strength;
                yUp = source.GetPixel(bx, by - 1).grayscale * strength;
                yDown = source.GetPixel(bx, by + 1).grayscale * strength;
                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;
                result.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, yDelta));
            }
        }
        result.Apply();
        result.wrapMode = TextureWrapMode.Clamp;

        Debug.Log("normal tex: " + stopwatch.Elapsed.TotalSeconds.ToString());
        return result;
    }
}

