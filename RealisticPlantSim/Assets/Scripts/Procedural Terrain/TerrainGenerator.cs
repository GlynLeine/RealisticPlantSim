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
    public Texture2D baseNormalTexture;
    public float normalStrength = 3;
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

        gridXpos = position.x;
        gridYPos = position.y;
        heightMap = CreateHeightmap(index.x, index.y);
        normalMap = heightMap.CreateNormal(TerrainGenerator.instance.normalStrength);

        heightMap = heightMap.Blend(TerrainGenerator.instance.baseHeightTexture, 0.5f, true);
        normalMap = normalMap.Blend(TerrainGenerator.instance.baseNormalTexture, 0.5f, true);
        chunkObject = createTerrain(size.x, size.y);
        SetActive(true);
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

        float width = TerrainGenerator.instance.textureWidth;
        float height = TerrainGenerator.instance.textureHeight;

        float chunkSize = TerrainGenerator.instance.maximumChunkSize;

        // Set Max Height for World
        float maxPossibleHeight = 0;
        float amplitude = TerrainGenerator.instance.PerlinBaseAmplitude;

        System.Random prng = new System.Random(TerrainGenerator.instance.Seed);
        float offsetX = prng.Next(-100000, 100000) - TerrainGenerator.instance.xOffset - (chunkSize * gridX);
        float offsetY = prng.Next(-100000, 100000) - TerrainGenerator.instance.yOffset - (chunkSize * gridY);

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
                    float px = (float)((x / width) * chunkSize + octaveOffsets[i].x) / TerrainGenerator.instance.PerlinScale * freq;
                    float py = (float)((y / height) * chunkSize + octaveOffsets[i].y) / TerrainGenerator.instance.PerlinScale * freq;

                    float PerlinValue = Mathf.PerlinNoise(px, py) * 2 - 1;
                    noiseHeight += PerlinValue * amplitude;

                    amplitude *= TerrainGenerator.instance.persistence;
                    freq *= TerrainGenerator.instance.lacunarity;
                }

                // Normalize Sample to fit world Sample Height
                float normalizedHeight = (noiseHeight + maxPossibleHeight) / (2f * maxPossibleHeight);
                float value = Mathf.Clamp(normalizedHeight, 0, float.MaxValue);

                if (TerrainGenerator.instance.includeSineWave)
                {
                    //combining sine wave with perlin noise
                    float sin = TerrainGenerator.instance.sinAmplitude * (Mathf.Sin(TerrainGenerator.instance.sinPeriod * ((x / width) * chunkSize + offsetX)) + 1f)/2f;

                    float maxAmp = 1f + TerrainGenerator.instance.perlinNoiseWeight;

                    value = Mathf.Clamp((sin + (value * TerrainGenerator.instance.perlinNoiseWeight)) / maxAmp, 0, float.MaxValue);
                }
                Color color = new Color(value, value, value);
                heightMapTexture.SetPixel(x, y, color);
            }
        }

        heightMapTexture.filterMode = FilterMode.Bilinear;
        heightMapTexture.wrapMode = TextureWrapMode.Clamp;
        heightMapTexture.Apply(false, false);

        Debug.Log("height tex: " + (stopwatch.Elapsed.TotalSeconds).ToString());
        return heightMapTexture;
    }


}

public static class ColorUtils
{
    struct Vector3Uint
    {
        public uint x, y, z;
        public Vector3Uint(uint x, uint y, uint z) { this.x = x; this.y = y; this.z = z; }
    };

    static ComputeShader m_blendShader;
    static int m_blendKernel;
    static Vector3Uint m_blendGlobal;

    static ComputeShader m_normalShader;
    static int m_normalKernel;
    static int m_blurKernel;
    static Vector3Uint m_normalGlobal;
    static Vector3Uint m_blurGlobal;

    public static Texture2D Blend(this Texture2D texture, Texture2D otherTexture, float amount, bool genMips = false)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!m_blendShader)
        {
            m_blendShader = Resources.Load<ComputeShader>("Compute/Blend");
            if (!m_blendShader)
            {
                Debug.LogError("Could not load blend compute shader.");
                return null;
            }

            m_blendKernel = m_blendShader.FindKernel("CSBlend");
            m_blendShader.GetKernelThreadGroupSizes(m_blendKernel, out m_blendGlobal.x, out m_blendGlobal.y, out m_blendGlobal.z);
        }

        m_blendShader.SetFloat("strength", amount);
        m_blendShader.SetInts("resolution", new int[] { texture.width, texture.height });

        RenderTexture rt = new RenderTexture(texture.width, texture.height, 1, texture.graphicsFormat);
        rt.enableRandomWrite = true;
        rt.Create();

        m_blendShader.SetTexture(m_blendKernel, "target", rt);
        m_blendShader.SetTexture(m_blendKernel, "sourceA", texture);
        m_blendShader.SetTexture(m_blendKernel, "sourceB", otherTexture);

        Vector3Int dispatchGroupSize = new Vector3Int(texture.width / (int)m_blendGlobal.x, texture.height / (int)m_blendGlobal.y, 1);
        m_blendShader.Dispatch(m_blendKernel, dispatchGroupSize.x, dispatchGroupSize.y, dispatchGroupSize.z);

        Texture2D result = new Texture2D(texture.width, texture.height, texture.format, false);

        RenderTexture.active = rt;

        result.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0, false);

        result.filterMode = texture.filterMode;
        result.anisoLevel = texture.anisoLevel;
        result.wrapModeU = texture.wrapModeU;
        result.wrapModeV = texture.wrapModeV;
        result.wrapModeW = texture.wrapModeW;

        result.Apply(genMips, !texture.isReadable);
        RenderTexture.active = null;
        rt.Release();

        Debug.Log("blend tex: " + stopwatch.Elapsed.TotalSeconds.ToString());
        return result;
    }

    public static Texture2D CreateNormal(this Texture2D source, float strength)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!m_normalShader)
        {
            m_normalShader = Resources.Load<ComputeShader>("Compute/Normal");
            if (!m_normalShader)
            {
                Debug.LogError("Could not load normal compute shader.");
                return null;
            }

            m_normalKernel = m_normalShader.FindKernel("CSNormal");
            m_blurKernel = m_normalShader.FindKernel("CSBlur");
            m_normalShader.GetKernelThreadGroupSizes(m_normalKernel, out m_normalGlobal.x, out m_normalGlobal.y, out m_normalGlobal.z);
            m_normalShader.GetKernelThreadGroupSizes(m_blurKernel, out m_blurGlobal.x, out m_blurGlobal.y, out m_blurGlobal.z);
        }

        strength = Mathf.Clamp(strength, 0.0F, 10.0F);

        RenderTexture rt = new RenderTexture(source.width, source.height, 1, source.graphicsFormat);
        rt.enableRandomWrite = true;
        rt.Create();

        m_normalShader.SetFloat("strength", strength);
        m_normalShader.SetTexture(m_normalKernel, "target", rt);
        m_normalShader.SetTexture(m_normalKernel, "source", source);
        m_normalShader.SetInts("resolution", new int[] { source.width, source.height });

        float chunkSize = TerrainGenerator.instance.maximumChunkSize;
        m_normalShader.SetFloats("texelSize", new float[] { chunkSize / source.width, chunkSize / source.height });

        Vector3Int dispatchGroupSize = new Vector3Int(source.width / (int)m_normalGlobal.x, source.height / (int)m_normalGlobal.y, 1);
        m_normalShader.Dispatch(m_normalKernel, dispatchGroupSize.x, dispatchGroupSize.y, dispatchGroupSize.z);

        RenderTexture blurRt = new RenderTexture(source.width, source.height, 1, source.graphicsFormat);
        blurRt.enableRandomWrite = true;
        blurRt.Create();

        m_normalShader.SetTexture(m_blurKernel, "target", blurRt);
        m_normalShader.SetTexture(m_blurKernel, "source", rt);
        m_normalShader.SetBool("horizontal", true);

        dispatchGroupSize = new Vector3Int(source.width / (int)m_blurGlobal.x, source.height / (int)m_blurGlobal.y, 1);
        m_normalShader.Dispatch(m_blurKernel, dispatchGroupSize.x, dispatchGroupSize.y, dispatchGroupSize.z);

        m_normalShader.SetTexture(m_blurKernel, "target", rt);
        m_normalShader.SetTexture(m_blurKernel, "source", blurRt);
        m_normalShader.SetBool("horizontal", false);

        dispatchGroupSize = new Vector3Int(source.width / (int)m_blurGlobal.x, source.height / (int)m_blurGlobal.y, 1);
        m_normalShader.Dispatch(m_blurKernel, dispatchGroupSize.x, dispatchGroupSize.y, dispatchGroupSize.z);

        Texture2D result = new Texture2D(source.width, source.height, source.format, false);

        RenderTexture.active = rt;

        result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);

        result.filterMode = source.filterMode;
        result.anisoLevel = source.anisoLevel;
        result.wrapModeU = source.wrapModeU;
        result.wrapModeV = source.wrapModeV;
        result.wrapModeW = source.wrapModeW;

        result.Apply(source.mipmapCount > 1, !source.isReadable);
        RenderTexture.active = null;
        rt.Release();
        blurRt.Release();

        Debug.Log("normal tex: " + stopwatch.Elapsed.TotalSeconds.ToString());
        return result;
    }
}

