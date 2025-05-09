using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class VoxelAStarPathing : MonoBehaviour
{

    [SerializeField] ChunkManager chunkManager;
    [SerializeField] Transform Target;
    public List<Vector3> PathFound = new List<Vector3>();

    private bool waitingForPath = false;

    public void Pathfind()
    {
        if (!waitingForPath)
        {
            waitingForPath = true;
            PathfindManager.Instance.RequestPath(this);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    PathFound.Clear();
        //    PathFound = VoxelPathfinding(transform.position, Target.position);
        //}
    }

    //public void Pathfind()
    //{
    //    if (!waitingForPath)
    //    {
    //        PathFound.Clear();
    //        PathFound = VoxelPathfinding(transform.position, Target.position);
    //    }
    //}

    public void PathfindNow()
    {
        waitingForPath = false;
        PathFound.Clear();
        PathFound = VoxelPathfinding(transform.position, Target.position);
    }

    // Move towards first node and once the ai reaches it, calculate pathfinding again
    public List<Vector3> VoxelPathfinding(Vector3 currentPos, Vector3 TargetPos)
    {
        Vector3 groundCurrent = GetVoxelPosition(currentPos);
        Vector3 groundTarget = GetVoxelPosition(TargetPos);

        PriorityQueue<VoxelPathNode> OpenNodes = new PriorityQueue<VoxelPathNode>();
        HashSet<Vector3> ClosedNodes = new HashSet<Vector3>();
        Dictionary<Vector3, VoxelPathNode> OpenDict = new Dictionary<Vector3, VoxelPathNode>();

        Dictionary<Vector3, VoxelPathNode> TotalDict = new Dictionary<Vector3, VoxelPathNode>();
        VoxelPathNode StartNode = new VoxelPathNode().PathInit(groundCurrent, groundCurrent, groundTarget);
        OpenNodes.Enqueue(StartNode, StartNode.FCost);
        OpenDict.Add(groundCurrent, StartNode);
        TotalDict.Add(groundCurrent, StartNode);

        const int maxNodesToExplore = 5000;
        int nodesExplored = 0;

        while (OpenNodes.Count > 0 && nodesExplored++ < maxNodesToExplore)
        {
            VoxelPathNode currentNode = OpenNodes.Dequeue();
            OpenDict.Remove(currentNode.pos);

            if (currentNode.pos == groundTarget)
            {
                return ReconstructPath(currentNode, groundCurrent, TotalDict);
            }

            foreach (Vector3 neighborDir in Container.voxelFaceChecks)
            {
                Vector3 NeighborPos = currentNode.pos + neighborDir;

                if (ClosedNodes.Contains(NeighborPos)) continue;

                Container container = chunkManager.FindChunkContainingVoxelOptimized(NeighborPos);
                if (container == null || container[NeighborPos - container.containerPosition].isSolid)
                {
                    continue;
                }

                int newGCost = currentNode.GCost + 1;

                if (OpenDict.TryGetValue(NeighborPos, out VoxelPathNode existingNode))
                {
                    if (newGCost < existingNode.GCost)
                    {
                        existingNode.GCost = newGCost;
                        existingNode.parentNodePos = currentNode.pos;
                        OpenNodes.Enqueue(existingNode, existingNode.FCost);
                    }
                }
                else
                {
                    VoxelPathNode newNode = new VoxelPathNode().PathInit(NeighborPos, groundCurrent, groundTarget);
                    newNode.parentNodePos = currentNode.pos;
                    OpenNodes.Enqueue(newNode, newNode.FCost);
                    OpenDict.Add(NeighborPos, newNode);
                    TotalDict.Add(NeighborPos, newNode);
                }
            }

            ClosedNodes.Add(currentNode.pos);
        }

        return new List<Vector3>();
    }

    private List<Vector3> ReconstructPath(VoxelPathNode endNode, Vector3 groundCurrent, Dictionary<Vector3, VoxelPathNode> TotalDict)
    {
        List<Vector3> path = new List<Vector3>();
        VoxelPathNode current = endNode;
        while (current.pos != groundCurrent)
        {
            path.Add(current.pos);
            TotalDict.TryGetValue(current.parentNodePos, out VoxelPathNode existingNode);
            current = existingNode;
        }
        path.Reverse();
        return path;
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
        if (PathFound == null || PathFound.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < PathFound.Count; i++)
        {
            Gizmos.DrawSphere(PathFound[i] + new Vector3(0.5f, 0.5f, 0.5f), 0.2f);

            if (i < PathFound.Count - 1)
            {
                Gizmos.DrawLine(PathFound[i] + new Vector3(0.5f, 0.5f, 0.5f), PathFound[i + 1] + new Vector3(0.5f, 0.5f, 0.5f));
            }
        }
    }
}

public struct VoxelPathNode
{
    public Vector3 pos;
    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
    public Vector3 parentNodePos;

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
public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T, int)>();

    public int Count => elements.Count;

    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
    }

    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].priority < elements[bestIndex].priority)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}


