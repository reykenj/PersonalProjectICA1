using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public Humanoid Player;
    [SerializeField] int MaxEnemies;
    [SerializeField] int TotalEnemies;
    [SerializeField] int MinEnemies;
    [SerializeField]
    private List<GameObject> EnemyPrefabs = new List<GameObject>();
    [SerializeField]
    private List<float> enemyHeights = new List<float>();

    public static GameFlowManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void PopulateMap()
    {
        float worldWidth = WorldManager.Instance.xSize * WorldManager.Instance.ChunkSize;
        float worldDepth = WorldManager.Instance.zSize * WorldManager.Instance.ChunkSize;

        for (int i = 0; i < MaxEnemies; i++)
        {
            float randX = Random.Range(0, worldWidth);
            float randZ = Random.Range(0, worldDepth);

            Vector3 rayOrigin = new Vector3(randX, worldDepth + 1, randZ); // shoot down from high above
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2000f, LayerMask.GetMask("Voxel")))
            {

                int Index = Random.Range(0, EnemyPrefabs.Count);
                Vector3 spawnPos = hit.point + Vector3.up * enemyHeights[Index];

                GameObject enemy = ObjectPool.GetObj(EnemyPrefabs[Index].name);
                enemy.transform.position = spawnPos;
            }
        }
        TotalEnemies = MaxEnemies;
    }
}
