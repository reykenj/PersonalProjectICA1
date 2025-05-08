using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class VoxelAStarPathing : MonoBehaviour
{

    [SerializeField] ChunkManager chunkManager;
    [SerializeField] Transform Target;
    private List<Vector3> debugPath = new List<Vector3>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            debugPath.Clear();
            debugPath = VoxelPathing(transform.position, Target.position);
        }
    }


    public List<Vector3> VoxelPathing(Vector3 currentPos, Vector3 TargetPos)
    {
        //Vector3 groundCurrent = GetGroundVoxelPosition(currentPos);
        //Vector3 groundTarget = GetGroundVoxelPosition(TargetPos);
        Vector3 groundCurrent = GetVoxelPosition(currentPos);
        Vector3 groundTarget = GetVoxelPosition(TargetPos);

        //groundCurrent.y++;
        //groundTarget.y++;
        List<VoxelPathNode> OpenNodes = new List<VoxelPathNode>();
        List<VoxelPathNode> ClosedNodes = new List<VoxelPathNode>();
        VoxelPathNode StartNode = new VoxelPathNode().PathInit(groundCurrent, groundCurrent, groundTarget);
        StartNode.parentNode = null;
        OpenNodes.Add(StartNode);
        int CurrentNodeIndex = 0;
        const int maxIterations = 10000;
        int iterations = 0;
        while (true)
        {
            if (OpenNodes.Count == 0 || iterations++ > maxIterations)
            {
                return new List<Vector3>();
            }

            int LowestFCost = int.MaxValue;
            for (int i = 0; i < OpenNodes.Count; i++)
            {
                if (OpenNodes[i].FCost < LowestFCost)
                {
                    LowestFCost = OpenNodes[i].FCost;
                    CurrentNodeIndex = i;
                }
            }

            if (OpenNodes[CurrentNodeIndex].pos == groundTarget)
            {
                VoxelPathNode currPath = OpenNodes[CurrentNodeIndex];
                List<Vector3> finalPath = new List<Vector3>();
                while (currPath.parentNode != null)
                {
                    finalPath.Add(currPath.pos);
                    currPath = currPath.parentNode;
                }

                finalPath.Reverse();
                return finalPath;
            }
            foreach (Vector3 neighborDir in Container.voxelFaceChecks)
            {
                //bool InClosed = false;
                //bool InOpen = false;
                Vector3 NeighborPos = OpenNodes[CurrentNodeIndex].pos + neighborDir;
                bool InClosed = ClosedNodes.Any(n => n.pos == NeighborPos);
                //for (int i = 0; i < ClosedNodes.Count; i++) {
                //    if(NeighborPos == ClosedNodes[i].pos)
                //    {
                //        InClosed = true;
                //    }
                //}
                Container container = chunkManager.FindChunkContainingVoxelOptimized(NeighborPos);
                if (container == null)
                {
                    Debug.Log("Container is null");
                }
                if (container == null || InClosed || container[NeighborPos - container.containerPosition].isSolid)
                {
                    continue;
                }

                int newGCost = OpenNodes[CurrentNodeIndex].GCost + 1;
                int existingIndex = OpenNodes.FindIndex(n => n.pos == NeighborPos);
                //for (int i = 0; i < OpenNodes.Count; i++)
                //{
                //    if (NeighborPos == OpenNodes[i].pos)
                //    {
                //        InOpen = true;
                //    }
                //}


                if (existingIndex == -1)
                {
                    VoxelPathNode newNode = new VoxelPathNode().PathInit(NeighborPos, groundCurrent, groundTarget);
                    newNode.parentNode = OpenNodes[CurrentNodeIndex];
                    OpenNodes.Add(newNode);
                }
                else if (newGCost < OpenNodes[existingIndex].GCost)
                {
                    VoxelPathNode updatedNode = OpenNodes[existingIndex];
                    //updatedNode = updatedNode.PathInit(NeighborPos, groundCurrent, groundTarget);
                    updatedNode.parentNode = OpenNodes[CurrentNodeIndex];
                    updatedNode.GCost = newGCost;
                    OpenNodes[existingIndex] = updatedNode;
                }
            }
            ClosedNodes.Add(OpenNodes[CurrentNodeIndex]);
            OpenNodes.Remove(OpenNodes[CurrentNodeIndex]);
        }

        //Container affectedChunk = FindChunkContainingVoxelOptimized(voxelPos);

        //if (affectedChunk != null)
        //{
        //    affectedChunk[voxelPos - affectedChunk.containerPosition] = Container.emptyVoxel;
        //    affectedChunk.GreedyMeshing();
        //    affectedChunk.UploadMesh();
        //}
    }

    private Vector3 GetGroundVoxelPosition(Vector3 startPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(startPos + Vector3.up * 10f, Vector3.down, out hit, 100f))
        {
            Vector3 hitPoint = hit.point - hit.normal * 0.01f;
            Vector3 voxelPos = new Vector3(
                Mathf.Floor(hitPoint.x),
                Mathf.Floor(hitPoint.y),
                Mathf.Floor(hitPoint.z));

            return voxelPos;
        }
        else
        {
            return startPos;
        }
    }

    private Vector3 GetVoxelPosition(Vector3 startPos)
    {
        Vector3 voxelPos = new Vector3(
        Mathf.Floor(startPos.x),
        Mathf.Floor(startPos.y),
        Mathf.Floor(startPos.z));
        return voxelPos;
    }

    private void OnDrawGizmos()
    {
        if (debugPath == null || debugPath.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < debugPath.Count; i++)
        {
            Gizmos.DrawSphere(debugPath[i] + new Vector3(0.5f, 0.5f, 0.5f), 0.2f);

            if (i < debugPath.Count - 1)
            {
                Gizmos.DrawLine(debugPath[i] + new Vector3(0.5f, 0.5f, 0.5f), debugPath[i + 1] + new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }
}

public class VoxelPathNode
{
    public Vector3 pos;
    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
    public VoxelPathNode parentNode;

    public VoxelPathNode PathInit(Vector3 VoxelPos, Vector3 StartPos, Vector3 EndPos)
    {
        pos = VoxelPos;
        GCost = (int)Vector3.Distance(StartPos, VoxelPos);
        HCost = (int)Vector3.Distance(EndPos, VoxelPos);
        return this;
    }

    bool Compare(VoxelPathNode other)
    {
        return pos == other.pos && GCost == other.GCost && HCost == other.HCost;
    }
}
