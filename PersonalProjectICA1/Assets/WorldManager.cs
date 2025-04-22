using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    public VoxelColor[] WorldColors;
    private Container container;
    int[] triangles;
    Color[] colourMap;
    public int xSize = 100;
    public int zSize = 100;

    public float noiseScale;
    public float noiseIntensity = 100.0f;
    public AnimationCurve meshHeightCurve;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    public TerrainType[] regions;

    private float lastNoiseScale;
    private int lastOctaves;
    private float lastPersistance;
    private float lastLacunarity;
    private int lastSeed;
    private Vector2 lastOffset;

    public bool UseFalloff;

    float[,] falloffmap;

    public float heightThreshold = 20f;


    [SerializeField] private GameObject treesPrefab;
    [SerializeField] int maxTrees = 20;
    [SerializeField] int minTrees = 10;

    [SerializeField] private GameObject seaPrefab;
    [SerializeField] private Vector3 seaOffset;
    [SerializeField] private Material shaderGraphMaterial;
    //


    void Start()
    {
        //GameObject cont = new GameObject("Container");
        //cont.transform.parent = transform;
        //container = cont.AddComponent<Container>();
        //container.Initialize(worldMaterial, Vector3.zero);

        //for (int x = 0; x < 16; x++)
        //{
        //    for (int z = 0; z < 16; z++)
        //    {
        //        int randomYHeight = Random.Range(1, 16);
        //        for (int y = 0; y < randomYHeight; y++)
        //        {
        //            container[new Vector3(x, y, z)] = new Voxel() { ID = 1 };
        //        }
        //    }
        //}


        //container.GenerateMesh();
        //container.UploadMesh();
        InitMap();
    }



    public void InitMap()
    {
        transform.position = new Vector3(-xSize * 0.5f, 0, -zSize * 0.5f);
        falloffmap = FalloffGenerator.GenerateFallOfMap(xSize + 1, zSize + 1);
        StorePreviousValues();
        GenerateMesh();

    }
    Color GetInterpolatedColor(float height)
    {
        if (regions.Length == 0) return Color.white;

        for (int i = 0; i < regions.Length - 1; i++)
        {
            if (height < regions[i + 1].height)
            {
                //float t = Mathf.InverseLerp(regions[i].height, regions[i + 1].height, height);
                float t = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(regions[i].height, regions[i + 1].height, height));

                return Color.Lerp(regions[i].colour, regions[i + 1].colour, t);
            }
        }
        return regions[regions.Length - 1].colour;
    }

    Texture2D GenerateSmoothGradientTexture(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, GetInterpolatedColor(heightMap[x, y]));
            }
        }

        texture.Apply();
        return texture;
    }

    //void GenerateMesh()
    //{
    //    float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, seed, noiseScale, octaves, persistance, lacunarity, offset);

    //    //vertices = new Vector3[(xSize + 1) * (zSize + 1)];
    //    colourMap = new Color[(xSize + 1) * (zSize + 1)];
    //    //uvs = new Vector2[vertices.Length];

    //    for (int i = 0, z = 0; z <= zSize; z++)
    //    {
    //        for (int x = 0; x <= xSize; x++)
    //        {
    //            if (UseFalloff)
    //            {
    //                noiseMap[x, z] = Mathf.Clamp01(noiseMap[x, z] - falloffmap[x, z]);
    //            }

    //            float y = meshHeightCurve.Evaluate(noiseMap[x, z]) * noiseIntensity;
    //            y = Mathf.Round(y);

    //            //vertices[i] = new Vector3(x - xSize * 0.5f, y, z - zSize * 0.5f); // note for voxel the 0.5 is the units that they are away from each other

    //            float normalizedHeight = noiseMap[x, z];
    //            colourMap[i] = GetInterpolatedColor(normalizedHeight);

    //            //uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);

    //            int surfaceY = Mathf.RoundToInt(meshHeightCurve.Evaluate(noiseMap[x, z]) * noiseIntensity);

    //            for (int voxely = 0; voxely <= surfaceY; voxely++)
    //            {
    //                GameObject cont = new GameObject("Container");
    //                cont.transform.parent = transform;
    //                cont.transform.position = new Vector3(x - xSize * 0.5f, voxely, z - zSize * 0.5f);

    //                container = cont.AddComponent<Container>();
    //                container.Initialize(worldMaterial, Vector3.zero);
    //                container.GenerateMesh();
    //                container.UploadMesh();
    //            }

    //            i++;
    //        }
    //    }



    //    int vert = 0;
    //    int tris = 0;
    //    triangles = new int[xSize * zSize * 6];
    //    for (int z = 0; z < zSize; z++)
    //    {
    //        for (int x = 0; x < xSize; x++)
    //        {
    //            triangles[tris + 0] = vert + 0;
    //            triangles[tris + 1] = vert + xSize + 1;
    //            triangles[tris + 2] = vert + 1;

    //            triangles[tris + 3] = vert + 1;
    //            triangles[tris + 4] = vert + xSize + 1;
    //            triangles[tris + 5] = vert + xSize + 2;
    //            vert++;
    //            tris += 6;
    //        }
    //        vert++;
    //    }

    //    //UpdateMesh();

    //    Texture2D texture = GenerateSmoothGradientTexture(noiseMap);
    //    ApplyTextureToMesh(texture);

    //}

    void GenerateMesh()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(xSize + 1, zSize + 1, seed, noiseScale, octaves, persistance, lacunarity, offset);

        GameObject cont = new GameObject("Container");
        cont.transform.parent = transform;
        container = cont.AddComponent<Container>();
        container.Initialize(worldMaterial, Vector3.zero);


        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                int surfaceY = Mathf.RoundToInt(meshHeightCurve.Evaluate(noiseMap[x, z]) * noiseIntensity);

                for (int y = 0; y <= surfaceY; y++)
                {
                    container[new Vector3(x, y, z)] = new Voxel() { ID = 1 };
                }
            }
        }
        container.GenerateMesh();
        container.UploadMesh();
    }

    // Helper method to check if a voxel exists at (x,y,z)
    private bool IsVoxelSolid(int x, int y, int z, float[,] noiseMap)
    {
        if (x < 0 || x > xSize || z < 0 || z > zSize || y < 0)
            return false; // Out of bounds = air

        float surfaceY = Mathf.RoundToInt(meshHeightCurve.Evaluate(noiseMap[x, z]) * noiseIntensity);
        return y <= surfaceY; // Solid if below/equal to surface
    }


    void ApplyTextureToMesh(Texture2D texture)
    {
        if (shaderGraphMaterial == null)
        {
            Debug.LogError("Shader Graph material is missing!");
            return;
        }

        Material materialInstance = new Material(shaderGraphMaterial);
        materialInstance.SetTexture("_MainTex", texture);
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = materialInstance;
    }

    void Update()
    {
        //if (HasSettingsChanged())
        //{
        //    StorePreviousValues();
        //    GenerateMesh();
        //}
    }

    private void StorePreviousValues()
    {
        lastNoiseScale = noiseScale;
        lastOctaves = octaves;
        lastPersistance = persistance;
        lastLacunarity = lacunarity;
        lastSeed = seed;
        lastOffset = offset;
    }

    private bool HasSettingsChanged()
    {
        return Mathf.Abs(noiseScale - lastNoiseScale) > Mathf.Epsilon ||
               octaves != lastOctaves ||
               Mathf.Abs(persistance - lastPersistance) > Mathf.Epsilon ||
               Mathf.Abs(lacunarity - lastLacunarity) > Mathf.Epsilon ||
               seed != lastSeed ||
               offset != lastOffset;
    }



    private static WorldManager _instance;

    public static WorldManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<WorldManager>();
            return _instance;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;

}


