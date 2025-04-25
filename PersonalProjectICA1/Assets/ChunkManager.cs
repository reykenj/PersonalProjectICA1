using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public GameObject voxelDebrisPrefab; 
    public List<Container> chunks;
    public Camera playerCamera;
    public float maxInteractionDistance = 10f;
    public LayerMask voxelLayerMask;


    [Header("Debug Visualization")]
    public bool showHitPoint = true;
    public Color hitPointColor = Color.red;
    public float hitPointRadius = 0.2f;
    public bool showRaycast = true;
    public Color raycastColor = Color.yellow;

    private Vector3 lastHitPoint;
    private bool hasHit = false;


    [Header("Explosion Settings")]
    public float explosionForce = 20f;
    public float explosionRadius = 4.5f;
    public float upwardModifier = 0.5f;
    public ForceMode forceMode = ForceMode.Impulse;
    public LayerMask physicsLayerMask;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RemoveVoxelAreaAtCameraLook();
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (showRaycast)
        {
            Gizmos.color = raycastColor;
            Vector3 rayOrigin = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).origin;
            Gizmos.DrawLine(rayOrigin, rayOrigin + playerCamera.transform.forward * maxInteractionDistance);
        }
        if (showHitPoint && hasHit)
        {
            Gizmos.color = hitPointColor;
            Gizmos.DrawSphere(lastHitPoint, hitPointRadius);
            Gizmos.color = Color.blue;
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out hit, maxInteractionDistance, voxelLayerMask))
            {
                Gizmos.DrawLine(hit.point, hit.point + hit.normal);
            }
        }
    }

    public void RemoveVoxelAtCameraLook()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance, voxelLayerMask))
        {
            hasHit = true;
            Vector3 hitPoint = hit.point - hit.normal * 0.01f; 
            Vector3 voxelPos = new Vector3(
                Mathf.Floor(hitPoint.x),
                Mathf.Floor(hitPoint.y),
                Mathf.Floor(hitPoint.z));
            lastHitPoint = voxelPos;
            Container affectedChunk = FindChunkContainingVoxelOptimized(voxelPos);

            if (affectedChunk != null)
            {
                affectedChunk[voxelPos - affectedChunk.containerPosition] = Container.emptyVoxel;
                affectedChunk.GreedyMeshing();
                affectedChunk.UploadMesh();
            }
        }
        else
        {
            hasHit = false;
        }
    }
    private Container FindChunkContainingVoxelOptimized(Vector3 voxelPosition)
    {
        int chunkSize = chunks[0].ChunkVoxelMaxAmtXZ;
        Vector3 chunkCoord = new Vector3(
            Mathf.Floor(voxelPosition.x / chunkSize) * chunkSize,
            Mathf.Floor(voxelPosition.y / chunkSize) * chunkSize,
            Mathf.Floor(voxelPosition.z / chunkSize) * chunkSize);
        foreach (Container chunk in chunks)
        {
            if (chunk.containerPosition == chunkCoord)
            {
                return chunk;
            }
        }
        return null;
    }




    public List<Vector4> RemoveVoxelsInArea(Vector3 center, float radius)
    {
        List<Vector4> Results = new List<Vector4>();

        Vector3 min = new Vector3(
            Mathf.Floor(center.x - radius),
            Mathf.Floor(center.y - radius),
            Mathf.Floor(center.z - radius));
        Vector3 max = new Vector3(
            Mathf.Ceil(center.x + radius),
            Mathf.Ceil(center.y + radius),
            Mathf.Ceil(center.z + radius));


        HashSet<Container> affectedChunks = new HashSet<Container>();
        for (float x = min.x; x <= max.x; x++)
        {
            for (float y = min.y; y <= max.y; y++)
            {
                for (float z = min.z; z <= max.z; z++)
                {
                    Vector3 voxelPos = new Vector3(x, y, z);
                    Container chunk = FindChunkContainingVoxelOptimized(voxelPos);
                    if (chunk != null)
                    {
                        affectedChunks.Add(chunk);
                    }
                }
            }
        }


        foreach (Container chunk in affectedChunks)
        {
            bool chunkModified = false;

            Vector3 chunkMin = chunk.containerPosition;
            Vector3 chunkMax = chunkMin + new Vector3(
                chunk.ChunkVoxelMaxAmtXZ,
                chunk.ChunkVoxelMaxAmtXZ,
                chunk.ChunkVoxelMaxAmtXZ);


            for (float x = Mathf.Max(min.x, chunkMin.x); x <= Mathf.Min(max.x, chunkMax.x - 1); x++)
            {
                for (float y = Mathf.Max(min.y, chunkMin.y); y <= Mathf.Min(max.y, chunkMax.y - 1); y++)
                {
                    for (float z = Mathf.Max(min.z, chunkMin.z); z <= Mathf.Min(max.z, chunkMax.z - 1); z++)
                    {
                        Vector3 voxelPos = new Vector3(x, y, z);
                        Vector3 localPos = voxelPos - chunk.containerPosition;


                        if (Vector3.Distance(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), center) <= radius)
                        {

                            if (chunk[localPos].isSolid)
                            {
                                Vector4 TruePos = voxelPos + new Vector3(0.5f, 0.5f, 0.5f);
                                TruePos.w = chunk[localPos].ID;
                                Results.Add(TruePos);
                                chunk[localPos] = Container.emptyVoxel;
                                chunkModified = true;
                            }
                        }
                    }
                }
            }


            if (chunkModified)
            {
                chunk.GreedyMeshing();
                chunk.UploadMesh();
            }
        }

        return Results; 
    }



    public void RemoveVoxelAreaAtCameraLook()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionDistance, voxelLayerMask))
        {
            hasHit = true;
            Vector3 hitPoint = hit.point - hit.normal * 0.01f;
            Vector3 voxelPos = new Vector3(
                Mathf.Floor(hitPoint.x),
                Mathf.Floor(hitPoint.y),
                Mathf.Floor(hitPoint.z));
            lastHitPoint = voxelPos;

            List<Vector4> affectedVoxels = RemoveVoxelsInArea(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), 3.5f);

            foreach (Vector4 voxelData in affectedVoxels)
            {
                Vector3 position = new Vector3(voxelData.x, voxelData.y, voxelData.z);
                int voxelID = (int)voxelData.w;

                if (voxelID > 0 && voxelID <= WorldManager.Instance.regions.Length)
                {
                    SpawnVoxelDebris(position, WorldManager.Instance.regions[voxelID - 1].colour);
                }
            }
            ApplyExplosionForce(hit.point);
        }
        else
        {
            hasHit = false;
        }
    }


    private void ApplyExplosionForce(Vector3 explosionCenter)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, physicsLayerMask);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(
                    explosionForce,
                    explosionCenter,
                    explosionRadius,
                    upwardModifier,
                    forceMode);
            }
        }
    }


    private void SpawnVoxelDebris(Vector3 position, VoxelColor voxelColor, float AngularVelocity = 5f)
    {
        if (voxelDebrisPrefab == null) return;

        GameObject debris = Instantiate(voxelDebrisPrefab, position, Quaternion.identity);
        debris.layer = 7;
        Renderer renderer = debris.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = voxelColor.color;
            renderer.material.SetFloat("_Metallic", voxelColor.metallic);
            renderer.material.SetFloat("_Glossiness", voxelColor.smoothness);
        }

        // Add some physics randomness
        Rigidbody rb = debris.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.angularVelocity = new Vector3(
                Random.Range(-AngularVelocity, AngularVelocity),
                Random.Range(-AngularVelocity, AngularVelocity),
                Random.Range(-AngularVelocity, AngularVelocity));
        }

        Destroy(debris, 5f);
    }



    /// LATER ALL THESE GET COMPONENTS WE SHOULD USE THE STRATEGY WE LEARNT FROM MK AND REY PROJECT ( WARNING LINE STRAT)
}