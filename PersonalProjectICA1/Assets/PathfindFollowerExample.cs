using System.Collections;
using UnityEngine;

public class PathfindFollowerExample : MonoBehaviour
{
    [SerializeField] VoxelAStarPathing VoxelAStarPathing;
    [SerializeField] float Movespeed;
    [SerializeField] float MaxSearchTimer = 1.0f;

    Coroutine FindNewPath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FindPath());
    }

    // Update is called once per frame
    void Update()
    {
        if(VoxelAStarPathing.PathFound.Count > 0)
        {
            Vector3 Diff = VoxelAStarPathing.PathFound[0] - transform.position;
            Vector3 Direction = Diff.normalized;
            if (Vector3.Distance(transform.position, VoxelAStarPathing.PathFound[0]) < 0.1)
            {
                VoxelAStarPathing.PathFound.RemoveAt(0);
            }
            transform.position += Direction * Movespeed * Time.deltaTime;
            //Debug.Log("GOING " + Direction);
        }
        //else
        //{
        //    SearchTimer -= Time.deltaTime;
        //    if (SearchTimer < 0)
        //    {
        //        SearchTimer = MaxSearchTimer + Random.Range(-0.25f, 0.25f);
        //        VoxelAStarPathing.Pathfind();
        //    }
        //    //Debug.Log("Searching");

        //    //VoxelAStarPathing.Pathfind();
        //}
    }

    IEnumerator FindPath()
    {
        while (true)
        {
            if (VoxelAStarPathing.PathFound.Count <= 0)
            {

                VoxelAStarPathing.Pathfind();
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }
}
