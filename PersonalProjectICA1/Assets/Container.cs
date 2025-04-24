using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Container : MonoBehaviour
{
    public Vector3 containerPosition;

    private Dictionary<Vector3, Voxel> data;
    private MeshData meshData = new MeshData();

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private int ChunkVoxelMaxAmtXZ;

    public void Initialize(Material mat, Vector3 position, int ChunkVoxelMaxAmt)
    {
        ConfigureComponents();
        data = new Dictionary<Vector3, Voxel>();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
        ChunkVoxelMaxAmtXZ = ChunkVoxelMaxAmt;
    }

    public void ClearData()
    {
        data.Clear();
    }

    public void GenerateMesh()
    {
        meshData.ClearData();

        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        VoxelColor voxelColor;
        Color voxelColorAlpha;
        Vector2 voxelSmoothness;
        foreach (KeyValuePair<Vector3, Voxel> kvp in data)
        {
            //Only check on solid blocks
            if (!kvp.Value.isSolid)
                continue;

            blockPos = kvp.Key;
            block = kvp.Value;

            voxelColor = WorldManager.Instance.regions[block.ID - 1].colour;
            voxelColorAlpha = voxelColor.color;
            voxelColorAlpha.a = 1;
            voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);
            //Iterate over each face direction
            for (int i = 0; i < 6; i++)
            {
                //Check if there's a solid block against this face
                if (this[blockPos + voxelFaceChecks[i]].isSolid)
                    continue;

                //Draw this face

                //Collect the appropriate vertices from the default vertices and add the block position
                for (int j = 0; j < 4; j++)
                {
                    faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + blockPos;
                    faceUVs[j] = voxelUVs[j];
                }

                for (int j = 0; j < 6; j++)
                {
                    meshData.vertices.Add(faceVertices[voxelTris[i, j]]);
                    meshData.UVs.Add(faceUVs[voxelTris[i, j]]);
                    meshData.colors.Add(voxelColorAlpha);
                    meshData.UVs2.Add(voxelSmoothness);

                    meshData.triangles.Add(counter++);

                }
            }

        }
    }

    public void GreedyMeshing()
    {
        meshData.ClearData();

        Vector3 blockPos;
        Voxel block;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];
        VoxelColor voxelColor;
        Color voxelColorAlpha;
        Vector2 voxelSmoothness;


        List<GreedyMesh> greedyMeshes;
        foreach (Vector3 direction in voxelFaceChecks)
        {
            bool[,,] visited = new bool[ChunkVoxelMaxAmtXZ, ChunkVoxelMaxAmtXZ, ChunkVoxelMaxAmtXZ];
            for (int x = 0; x < ChunkVoxelMaxAmtXZ; x++)
            {
                for (int y = 0; y < ChunkVoxelMaxAmtXZ; y++)
                {
                    for (int z = 0; z < ChunkVoxelMaxAmtXZ; z++)
                    {
                        blockPos = new Vector3(x, y, z);
                        block = this[blockPos];
                        //Only check on solid blocks
                        if (!block.isSolid || visited[x, y, z])
                            continue;
                        AxisInfo axisInfo = directionToAxis[direction];
                        GreedyMesh greedyMesh = new GreedyMesh();
                        greedyMesh.Direction = direction;
                        greedyMesh.StartPosition = blockPos;
                        greedyMesh.HeightWidth[axisInfo.u] = 1;
                        greedyMesh.HeightWidth[axisInfo.v] = 1;
                        bool widthdone = false;
                        bool heightdone = false;
                        Vector3 CurrentCheck = greedyMesh.StartPosition + greedyMesh.HeightWidth;
                        while (true)
                        {
                            // TO DO THIS SHIT INCOMPLETE CURRENT CHECK MAY NOT BE PERFECTLY IN THE RIGHT DIRECTION,
                            // EVERYTHING IN THIS WHILE LOOP MAY NOT BE IN THE RIGHT DIRECTION
                            if (!widthdone)
                            {
                                //CurrentCheck[axisInfo.v] = 0; add tbese when enabling widht done or height done
                                CurrentCheck[axisInfo.u]++;
                            }
                            else if (!heightdone)
                            {
                                //CurrentCheck[axisInfo.u] = 0;
                                CurrentCheck[axisInfo.v]++;
                            }


                            if (this[CurrentCheck].isSolid &&
                                !visited[(int)CurrentCheck.x, (int)CurrentCheck.y, (int)CurrentCheck.z] &&
                                this[CurrentCheck + direction].isSolid)
                            {
                            }

                            //for (greedyX < greedyMesh.HeightWidth[axisInfo.u] * 2; greedyX++)
                            //{
                            //}
                            //Vector3 CurrentCheck = greedyMesh.StartPosition + (Vector3)greedyMesh.HeightWidth;
                            //if (this[CurrentCheck].isSolid && 
                            //    !visited[(int)CurrentCheck.x, (int)CurrentCheck.y, (int)CurrentCheck.z] &&
                            //    this[CurrentCheck + direction].isSolid)
                            //{
                            //    if (widthdone)
                            //    {
                            //        greedyMesh.HeightWidth[axisInfo.u]++;
                            //    }
                            //    else
                            //    {
                            //        greedyMesh.HeightWidth[axisInfo.v]++;
                            //    }
                            //}
                            //else if(!widthdone)
                            //{
                            //    widthdone = true;
                            //}
                            //else if (!heightdone)
                            //{
                            //    break;
                            //}


                        }
                    }
                }
            }
        }
    }


    struct AxisInfo
    {
        public int u; // First 2D axis index
        public int v; // Second 2D axis index
        public int w; // Fixed axis index

        public AxisInfo(int u, int v, int w)
        {
            this.u = u;
            this.v = v;
            this.w = w;
        }
    }


    //voxelColor = WorldManager.Instance.regions[block.ID - 1].colour;
    //voxelColorAlpha = voxelColor.color;
    //voxelColorAlpha.a = 1;
    //voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);
    ////Iterate over each face direction
    //for (int i = 0; i < 6; i++)
    //{
    //    //Check if there's a solid block against this face
    //    if (this[blockPos + voxelFaceChecks[i]].isSolid)
    //        continue;

    //    //Draw this face

    //    //Collect the appropriate vertices from the default vertices and add the block position
    //    for (int j = 0; j < 4; j++)
    //    {
    //        faceVertices[j] = voxelVertices[voxelVertexIndex[i, j]] + blockPos;
    //        faceUVs[j] = voxelUVs[j];
    //    }

    //    for (int j = 0; j < 6; j++)
    //    {
    //        meshData.vertices.Add(faceVertices[voxelTris[i, j]]);
    //        meshData.UVs.Add(faceUVs[voxelTris[i, j]]);
    //        meshData.colors.Add(voxelColorAlpha);
    //        meshData.UVs2.Add(voxelSmoothness);

    //        meshData.triangles.Add(counter++);

    //    }
    //}



    public void UploadMesh()
    {
        meshData.UploadMesh();

        if (meshRenderer == null)
            ConfigureComponents();

        meshFilter.mesh = meshData.mesh;
        if (meshData.vertices.Count > 3)
            meshCollider.sharedMesh = meshData.mesh;
    }

    private void ConfigureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public Voxel this[Vector3 index]
    {
        get
        {
            if (data.ContainsKey(index))
                return data[index];
            else
                return emptyVoxel;
        }

        set
        {
            if (data.ContainsKey(index))
                data[index] = value;
            else
                data.Add(index, value);
        }
    }

    public static Voxel emptyVoxel = new Voxel() { ID = 0 };

    #region Mesh Data

    public struct GreedyMesh
    {
        public Vector3 StartPosition;
        public Vector3 HeightWidth;
        public Vector3 Direction;
    }
    public struct MeshData
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> UVs;
        public List<Vector2> UVs2;
        public List<Color> colors;
        public bool Initialized;

        public void ClearData()
        {
            if (!Initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                UVs = new List<Vector2>();
                UVs2 = new List<Vector2>();
                colors = new List<Color>();

                Initialized = true;
                mesh = new Mesh();
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                UVs.Clear();
                UVs2.Clear();
                colors.Clear();

                mesh.Clear();
            }
        }
        public void UploadMesh(bool sharedVertices = false)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetColors(colors);

            mesh.SetUVs(0, UVs);
            mesh.SetUVs(2, UVs2);

            mesh.Optimize();

            mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            mesh.UploadMeshData(false);
        }
    }
    #endregion

    #region Static Variables
    static readonly Vector3[] voxelVertices = new Vector3[8]
    {
            new Vector3(0,0,0),//0
            new Vector3(1,0,0),//1
            new Vector3(0,1,0),//2
            new Vector3(1,1,0),//3

            new Vector3(0,0,1),//4
            new Vector3(1,0,1),//5
            new Vector3(0,1,1),//6
            new Vector3(1,1,1),//7
    };

    static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
            new Vector3(0,0,-1),//back
            new Vector3(0,0,1),//front
            new Vector3(-1,0,0),//left
            new Vector3(1,0,0),//right
            new Vector3(0,-1,0),//bottom
            new Vector3(0,1,0)//top
    };

    static readonly int[,] voxelVertexIndex = new int[6, 4]
    {
            {0,1,2,3},
            {4,5,6,7},
            {4,0,6,2},
            {5,1,7,3},
            {0,1,4,5},
            {2,3,6,7},
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,0),
            new Vector2(1,1)
    };

    static readonly int[,] voxelTris = new int[6, 6]
    {
            {0,2,3,0,3,1},
            {0,1,2,1,3,2},
            {0,2,3,0,3,1},
            {0,1,2,1,3,2},
            {0,1,2,1,3,2},
            {0,2,3,0,3,1},
    };


    // Map direction to its 2D mask axes
    static readonly Dictionary<Vector3, AxisInfo> directionToAxis = new Dictionary<Vector3, AxisInfo>
{
    { new Vector3(0, 0, 1), new AxisInfo(0, 1, 2) }, // Front
    { new Vector3(0, 0, -1), new AxisInfo(0, 1, 2) }, // Back
    { new Vector3(-1, 0, 0), new AxisInfo(2, 1, 0) }, // Left
    { new Vector3(1, 0, 0), new AxisInfo(2, 1, 0) }, // Right
    { new Vector3(0, 1, 0), new AxisInfo(0, 2, 1) }, // Top
    { new Vector3(0, -1, 0), new AxisInfo(0, 2, 1) }, // Bottom
};
    #endregion
}

