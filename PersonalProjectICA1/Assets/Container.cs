using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Container;
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

    public int ChunkVoxelMaxAmtXZ;

    public void Initialize(Material mat, Vector3 position, int ChunkVoxelMaxAmt)
    {
        ConfigureComponents();
        gameObject.layer = 6;
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

        int Faces = 0; /// TEST VALUE NOTHJING IMPORTANT
        int TotalFaces = 0; /// TEST VALUE NOTHJING IMPORTANT
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
                TotalFaces++;
                //Check if there's a solid block against this face
                if (this[blockPos + voxelFaceChecks[i]].isSolid)
                    continue;

                //Draw this face

                Faces++;
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

        Debug.Log("Faces (Culled): " + Faces);
        Debug.Log("Faces (Total): " + TotalFaces);
    }

    public void GreedyMeshing()
    {
        meshData.ClearData();

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        bool[,,] visited = new bool[ChunkVoxelMaxAmtXZ * 2, ChunkVoxelMaxAmtXZ * 2, ChunkVoxelMaxAmtXZ * 2];

        for (int directionIndex = 0; directionIndex < voxelFaceChecks.Length; directionIndex++)
        {
            Vector3 direction = voxelFaceChecks[directionIndex];
            AxisInfo axisInfo = directionToAxis[direction];

            Array.Clear(visited, 0, visited.Length);

            for (int u = 0; u < ChunkVoxelMaxAmtXZ * 2; u++)
            {
                for (int v = 0; v < ChunkVoxelMaxAmtXZ * 2; v++)
                {
                    for (int w = 0; w < ChunkVoxelMaxAmtXZ * 2; w++)
                    {
                        Vector3 blockPos = Vector3.zero;
                        blockPos[axisInfo.u] = u;
                        blockPos[axisInfo.v] = v;
                        blockPos[axisInfo.w] = w;

                        if (visited[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] || !this[blockPos].isSolid)
                            continue;

                        if (this[blockPos + direction].isSolid)
                            continue;

                        GreedyMesh greedyMesh = new GreedyMesh();
                        greedyMesh.Direction = direction;
                        greedyMesh.StartPosition = blockPos;
                        greedyMesh.HeightWidth = Vector3.one;
                        for (int du = 1; u + du < ChunkVoxelMaxAmtXZ; du++)
                        {
                            Vector3 nextPos = blockPos;
                            nextPos[axisInfo.u] += du;

                            if (!this[nextPos].isSolid || visited[(int)nextPos.x, (int)nextPos.y, (int)nextPos.z] ||
                                this[nextPos + direction].isSolid)
                                break;

                            greedyMesh.HeightWidth[axisInfo.u] += 1;
                        }

                        for (int dv = 1; v + dv < ChunkVoxelMaxAmtXZ; dv++)
                        {
                            bool rowValid = true;
                            for (int du = 0; du < greedyMesh.HeightWidth[axisInfo.u]; du++)
                            {
                                Vector3 checkPos = blockPos;
                                checkPos[axisInfo.u] += du;
                                checkPos[axisInfo.v] += dv;

                                if (!this[checkPos].isSolid || visited[(int)checkPos.x, (int)checkPos.y, (int)checkPos.z] ||
                                    this[checkPos + direction].isSolid)
                                {
                                    rowValid = false;
                                    break;
                                }
                            }

                            if (!rowValid)
                                break;

                            greedyMesh.HeightWidth[axisInfo.v] += 1;
                        }
                        for (int du = 0; du < greedyMesh.HeightWidth[axisInfo.u]; du++)
                        {
                            for (int dv = 0; dv < greedyMesh.HeightWidth[axisInfo.v]; dv++)
                            {
                                Vector3 markPos = blockPos;
                                markPos[axisInfo.u] += du;
                                markPos[axisInfo.v] += dv;
                                visited[(int)markPos.x, (int)markPos.y, (int)markPos.z] = true;
                            }
                        }

                        AddGreedyFace(greedyMesh, ref counter, faceVertices, faceUVs, directionIndex);
                    }
                }
            }
        }
    }


    private Vector3 GetPositionFromUV(int u, int v, AxisInfo axisInfo)
    {
        Vector3 pos = Vector3.zero;
        pos[axisInfo.u] = u;
        pos[axisInfo.v] = v;
        pos[axisInfo.w] = (axisInfo.w == 1) ? 0 : ChunkVoxelMaxAmtXZ - 1; // Adjust based on face direction
        return pos;
    }

    private void AddGreedyFace(GreedyMesh greedyMesh, ref int counter, Vector3[] faceVertices, Vector2[] faceUVs, int directionIndex)
    {
        AxisInfo axisInfo = directionToAxis[greedyMesh.Direction];

        // Get voxel properties
        Voxel block = this[greedyMesh.StartPosition];
        VoxelColor voxelColor = WorldManager.Instance.regions[block.ID - 1].colour;
        Color voxelColorAlpha = voxelColor.color;
        voxelColorAlpha.a = 1;
        Vector2 voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);

        // Convert to face vertices based on direction
        for (int j = 0; j < 4; j++)
        {
            faceVertices[j] = Vector3.Scale(voxelVertices[voxelVertexIndex[directionIndex, j]], greedyMesh.HeightWidth) + greedyMesh.StartPosition;
            faceUVs[j] = voxelUVs[j];
        }

        // Add to mesh
        for (int j = 0; j < 6; j++)
        {
            int triIndex = voxelTris[directionIndex, j];
            meshData.vertices.Add(faceVertices[triIndex]);
            meshData.UVs.Add(faceUVs[triIndex]);
            meshData.colors.Add(voxelColorAlpha);
            meshData.UVs2.Add(voxelSmoothness);
            meshData.triangles.Add(counter++);
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

