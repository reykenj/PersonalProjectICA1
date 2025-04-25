using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] bool GreedyMeshing;
    [SerializeField] ChunkManager chunkManager;
    public Material worldMaterial;
    private Container container;
    int[] triangles;
    Color[] colourMap;
    public int ChunkSize = 16;
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
        InitMap();
    }



    public void InitMap()
    {
        transform.position = new Vector3(-xSize * 0.5f, 0, -zSize * 0.5f);
        falloffmap = FalloffGenerator.GenerateFallOfMap(xSize + 1, zSize + 1);
        StorePreviousValues();
        GenerateMesh();

    }

    void GenerateMesh()
    {
        // Calculate total world size in voxels
        int worldWidth = xSize * ChunkSize;
        int worldDepth = zSize * ChunkSize;

        // Generate noise for entire world
        float[,] noiseMap = Noise.GenerateNoiseMap(worldWidth + 1, worldDepth + 1, seed, noiseScale, octaves, persistance, lacunarity, offset);

        // Create chunks
        for (int chunkZ = 0; chunkZ < ChunkSize; chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkSize; chunkX++)
            {
                GameObject cont = new GameObject($"Container_{chunkX}_{chunkZ}");
                cont.transform.parent = transform;
                cont.transform.position = new Vector3(chunkX * xSize, 0, chunkZ * zSize);

                Container container = cont.AddComponent<Container>();
                container.Initialize(worldMaterial, cont.transform.position, xSize);

                // Fill chunk with voxels
                for (int z = 0; z < xSize; z++)
                {
                    for (int x = 0; x < zSize; x++)
                    {
                        // Calculate world position
                        int worldX = chunkX * xSize + x;
                        int worldZ = chunkZ * zSize + z;

                        // Get height from noise map
                        int surfaceY = Mathf.RoundToInt(meshHeightCurve.Evaluate(noiseMap[worldX, worldZ]) * noiseIntensity);
                        // Fill column
                        for (int y = 0; y <= surfaceY; y++)
                        {
                            int assignedID = 0; 
                            for (int i = 0; i < regions.Length; i++)
                            {
                                if (regions[i].height < y)
                                {
                                    assignedID = i + 1;
                                }
                            }
                            container[new Vector3(x, y, z)] = new Voxel() { 
                                ID = (byte)assignedID
                            };
                        }
                    }
                }

                if (!GreedyMeshing)
                {
                    container.GenerateMesh();
                }
                else
                {
                    container.GreedyMeshing();
                }
                container.UploadMesh();
                chunkManager.chunks.Add(container);
            }
        }
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
    public VoxelColor colour;

}


