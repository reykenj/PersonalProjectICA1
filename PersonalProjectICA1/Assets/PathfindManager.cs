using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindManager : MonoBehaviour
{
    public static PathfindManager Instance;

    private Queue<VoxelAStarPathing> pathRequestQueue = new Queue<VoxelAStarPathing>();
    [SerializeField] int maxPathsPerCheck = 4;
    [SerializeField] float AmountOfSecondsPerCheck = 0.25f;
    [SerializeField] bool CoroutineEnable = false;

    Coroutine Pathfind;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (CoroutineEnable)
        {
            Pathfind = StartCoroutine(PathfindChecks());
        }
    }
    void OnDisable()
    {
        if (Pathfind != null)
        {
            StopCoroutine(Pathfind);
            Pathfind = null;
        }
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
        if (CoroutineEnable)
        {
            return;
        }
        int processed = 0;
        while (pathRequestQueue.Count > 0 && processed < maxPathsPerCheck)
        {
            var requester = pathRequestQueue.Dequeue();
            requester.PathfindNow();
            processed++;

            //Debug.Log("Processed Pathfinding");
        }
    }

    IEnumerator PathfindChecks()
    {
        while (true)
        {
            int processed = 0;
            while (pathRequestQueue.Count > 0 && processed < maxPathsPerCheck)
            {
                var requester = pathRequestQueue.Dequeue();
                requester.PathfindNow();
                processed++;
            }
            yield return new WaitForSeconds(AmountOfSecondsPerCheck);
        }
    }
}
