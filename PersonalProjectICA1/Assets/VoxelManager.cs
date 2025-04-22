using System.Collections.Generic;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    public static VoxelManager Instance;

    // Dictionary of voxel positions and their GameObjects
    private Dictionary<Vector3Int, Voxel> voxelMap = new();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterVoxel(Vector3Int pos, Voxel voxel)
    {
        voxelMap[pos] = voxel;
    }

    public void UnregisterVoxel(Vector3Int pos)
    {
        voxelMap.Remove(pos);
    }

    public Voxel GetVoxel(Vector3Int pos)
    {
        voxelMap.TryGetValue(pos, out var voxel);
        return voxel;
    }
}
