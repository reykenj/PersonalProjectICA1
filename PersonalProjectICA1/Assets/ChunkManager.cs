using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
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

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RemoveVoxelAtCameraLook();
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
}