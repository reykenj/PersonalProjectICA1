using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Collections;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [SerializeField] bool GreedyMeshing;
    [SerializeField] ChunkManager chunkManager;
    public Material worldMaterial;
    //private Container container;
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
    public bool RandomSeed;
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
        StartCoroutine(populate());
    }

    IEnumerator populate()
    {
        yield return new WaitForSeconds(0.25f);
        GameFlowManager.instance.PopulateMap();
    }

    public void InitMap()
    {
        transform.position = new Vector3(-xSize * 0.5f, 0, -zSize * 0.5f);
        falloffmap = FalloffGenerator.GenerateFallOfMap(xSize + 1, zSize + 1);
        //StorePreviousValues();
        GenerateMesh();

    }

    void GenerateMesh()
    {
        if (RandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        int worldWidth = xSize * ChunkSize;
        int worldDepth = zSize * ChunkSize;
        float[,] noiseMap = Noise.GenerateNoiseMap(worldWidth + 1, worldDepth + 1, seed, noiseScale, octaves, persistance, lacunarity, offset);
        for (int chunkZ = 0; chunkZ < ChunkSize; chunkZ++)
        {
            for (int chunkX = 0; chunkX < ChunkSize; chunkX++)
            {
                GameObject cont = new GameObject($"Container_{chunkX}_{chunkZ}");
                cont.transform.parent = transform;
                cont.transform.position = new Vector3(chunkX * xSize, 0, chunkZ * zSize);

                Container container = cont.AddComponent<Container>();
                container.Initialize(worldMaterial, cont.transform.position, xSize, ChunkSize);

                for (int z = 0; z < xSize; z++)
                {
                    for (int x = 0; x < zSize; x++)
                    {
                        int worldX = chunkX * xSize + x;
                        int worldZ = chunkZ * zSize + z;

                        int surfaceY = Mathf.Clamp(Mathf.RoundToInt(meshHeightCurve.Evaluate(noiseMap[worldX, worldZ]) * noiseIntensity), 0, xSize * 2 - 1);


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
                            container[new Vector3(x, y, z)] = new Voxel()
                            {
                                ID = (byte)assignedID
                            };
                        }
                    }
                }

                TryPlaceHouseInChunk(container);

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

    private bool TryPlaceHouseInChunk(Container chunk)
    {

        int houseWidth = Random.Range(5, 10);
        int houseDepth = Random.Range(5, 10);
        int houseHeight = Random.Range(4, 6);


        for (int attempt = 0; attempt < 5; attempt++)
        {

            int x = Random.Range(2, xSize - houseWidth - 2);
            int z = Random.Range(2, zSize - houseDepth - 2);


            int surfaceY = GetSurfaceHeight(chunk, x, z);
            Vector3 houseBase = new Vector3(x, surfaceY + 1, z);


            if (IsAreaFlat(chunk, houseBase, houseWidth, houseDepth))
            {
                GenerateSimpleHouse(chunk, houseBase, houseWidth, houseHeight, houseDepth);
                return true;
            }
        }
        return false;
    }

    private int GetSurfaceHeight(Container chunk, int x, int z)
    {
        for (int y = Container.ChunkVoxelMaxAmtXZ - 1; y >= 0; y--)
        {
            if (chunk[new Vector3(x, y, z)].ID != 0)
            {
                return y;
            }
        }
        return 0;
    }

    private bool IsAreaFlat(Container chunk, Vector3 center, int width, int depth)
    {
        int firstY = (int)center.y;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector3 checkPos = center + new Vector3(x, 0, z);
                int surfaceY = GetSurfaceHeight(chunk, (int)checkPos.x, (int)checkPos.z);

                if (Mathf.Abs(surfaceY - (firstY - 1)) > 1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void GenerateSimpleHouse(Container chunk, Vector3 basePos, int width, int height, int depth)
    {


        int minX = Mathf.Max(1, Mathf.FloorToInt(basePos.x - width / 2));
        int maxX = Mathf.Min(Container.ChunkVoxelMaxAmtXZ - 2, Mathf.CeilToInt(basePos.x + width / 2));
        int minZ = Mathf.Max(1, Mathf.FloorToInt(basePos.z - depth / 2));
        int maxZ = Mathf.Min(Container.ChunkVoxelMaxAmtXZ - 2, Mathf.CeilToInt(basePos.z + depth / 2));


        width = maxX - minX;
        depth = maxZ - minZ;
        Vector3 adjustedCenter = new Vector3(
            minX + width / 2f,
            basePos.y - 1,
            minZ + depth / 2f
        );

        byte wallBlockID = (byte)(SearchBlockIndex("WoodPlank") + 1);
        byte ceilingBlockID = (byte)(SearchBlockIndex("WoodCeiling") + 1);
        FillBox(chunk, adjustedCenter, width, height, depth, wallBlockID, hollow: true);


        int doorX = Mathf.Clamp(
            (int)adjustedCenter.x,
            minX + 1,
            maxX - 1
        );
        int doorZ = minZ;
        chunk[new Vector3(doorX, adjustedCenter.y + 1, doorZ)] = new Voxel() { ID = 0 };
        chunk[new Vector3(doorX, adjustedCenter.y + 2, doorZ)] = new Voxel() { ID = 0 };


        //int roofOverhang = 1;
        for (int i = 0; i < 4; i++)
        {
            for (int x = minX + i; x < maxX - i; x++)
            {
                for (int z = minZ + i; z < maxZ - i; z++)
                {
                    if (x >= 0 && x < Container.ChunkVoxelMaxAmtXZ &&
                        z >= 0 && z < Container.ChunkVoxelMaxAmtXZ)
                    {
                        chunk[new Vector3(x, adjustedCenter.y + height + i, z)] =
                            new Voxel() { ID = ceilingBlockID };
                    }
                }
            }
        }
    }

    void FillBox(Container chunk, Vector3 center, int width, int height, int depth, byte blockID, bool hollow = false)
    {
        int minX = Mathf.FloorToInt(center.x - width / 2f);
        int maxX = Mathf.CeilToInt(center.x + width / 2f);
        int minZ = Mathf.FloorToInt(center.z - depth / 2f);
        int maxZ = Mathf.CeilToInt(center.z + depth / 2f);

        for (int x = minX; x < maxX; x++)
        {
            for (int y = (int)center.y; y < (int)center.y + height; y++)
            {
                for (int z = minZ; z < maxZ; z++)
                {

                    if (x >= 0 && x < Container.ChunkVoxelMaxAmtXZ &&
                        y >= 0 && y < Container.ChunkVoxelMaxAmtXZ &&
                        z >= 0 && z < Container.ChunkVoxelMaxAmtXZ)
                    {
                        bool isWall = x == minX || x == maxX - 1 ||
                                     y == (int)center.y || y == (int)center.y + height - 1 ||
                                     z == minZ || z == maxZ - 1;

                        if (!hollow || isWall)
                        {
                            chunk[new Vector3(x, y, z)] = new Voxel() { ID = blockID };
                        }
                        else
                        {
                            chunk[new Vector3(x, y, z)] = new Voxel() { ID = 0 };
                        }
                    }
                }
            }
        }
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

    public int SearchBlockIndex(string Name)
    {
        for (int i = 0; i < regions.Length; i++)
        {
            if (regions[i].name == Name)
            {
                return i;
            }
        }
        return -1;
    }

    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public VoxelColor colour;

    }
}


