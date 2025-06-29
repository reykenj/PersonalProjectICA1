using System.Collections.Generic;
using UnityEngine;

public class PathfindManager : MonoBehaviour
{
    public static PathfindManager Instance;

    private Queue<VoxelAStarPathing> pathRequestQueue = new Queue<VoxelAStarPathing>();
    [SerializeField] int maxPathsPerFrame = 4;

    void Awake()
    {
        Instance = this;
    }

    public void RequestPath(VoxelAStarPathing requester)
    {
        //Debug.Log("REQUESTING Pathfinding");
        if (!pathRequestQueue.Contains(requester))
        {
            pathRequestQueue.Enqueue(requester);
            //Debug.Log("REQUESTING Pathfinding SUCCESS");
        }

    }

    void FixedUpdate()
    {
        int processed = 0;
        while (pathRequestQueue.Count > 0 && processed < maxPathsPerFrame)
        {
            var requester = pathRequestQueue.Dequeue();
            requester.PathfindNow();
            processed++;

            //Debug.Log("Processed Pathfinding");
        }
    }
}
