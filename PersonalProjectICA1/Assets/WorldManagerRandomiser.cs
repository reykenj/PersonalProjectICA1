using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class WorldManagerRandomiser : MonoBehaviour
{
    [SerializeField] List<WorldManager> WMs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        WMs[Random.Range(0, WMs.Count)].gameObject.SetActive(true);
    }
}
