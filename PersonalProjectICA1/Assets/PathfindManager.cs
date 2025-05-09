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
        if (!pathRequestQueue.Contains(requester))
            pathRequestQueue.Enqueue(requester);
    }

    void FixedUpdate()
    {
        int processed = 0;
        while (pathRequestQueue.Count > 0 && processed < maxPathsPerFrame)
        {
            var requester = pathRequestQueue.Dequeue();
            requester.PathfindNow();
            processed++;
        }
    }
}
