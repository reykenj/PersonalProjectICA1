using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.ParticleSystem;

public class ChunkManager : MonoBehaviour
{
    public GameObject voxelDebrisPrefab; 
    public List<Container> chunks;
    public Queue<Container> dirtyChunks = new Queue<Container>();
    public Camera playerCamera;
    public float maxInteractionDistance = 10f;
    public LayerMask voxelLayerMask;
    public ParticleSystem particleSystem;
    public List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();

    [SerializeField] bool CoroutineEnable = false;
    [SerializeField] float UpdTime = 0.1f;
    Coroutine TimedFixedUpd;



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

    public static ChunkManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    void OnEnable()
    {
        if (CoroutineEnable)
        {
            TimedFixedUpd = StartCoroutine(TimedFixedUpdate());
        }
    }
    void OnDisable()
    {
        if (TimedFixedUpd != null)
        {
            StopCoroutine(TimedFixedUpd);
            TimedFixedUpd = null;
        }
    }
    IEnumerator TimedFixedUpdate()
    {
        while (true)
        {
            if (dirtyChunks.Count > 0)
            {
                var requester = dirtyChunks.Dequeue();
                requester.GreedyMeshing();
                requester.UploadMesh();
            }
            yield return new WaitForSeconds(UpdTime);
        }
    }
    void FixedUpdate()
    {
        if (CoroutineEnable)
        {
            return;
        }
        if(dirtyChunks.Count > 0)
        {
            var requester = dirtyChunks.Dequeue();
            requester.GreedyMeshing();
            requester.UploadMesh();
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
                if (!dirtyChunks.Contains(affectedChunk))
                {
                    dirtyChunks.Enqueue(affectedChunk);
                }
            }
        }
        else
        {
            hasHit = false;
        }
    }
    public Container FindChunkContainingVoxelOptimized(Vector3 voxelPosition)
    {
        int chunkSize = Container.ChunkVoxelMaxAmtXZ;
        Vector3 chunkCoord = new Vector3(
            Mathf.Floor(voxelPosition.x / chunkSize) * chunkSize,
            0,
            Mathf.Floor(voxelPosition.z / chunkSize) * chunkSize);

        if( chunkCoord.x < 0 || chunkCoord.z < 0 || chunkCoord.x > Container.ChunkVoxelMaxAmtXZ * Container.ChunkSize || chunkCoord.z > Container.ChunkVoxelMaxAmtXZ * Container.ChunkSize)
        {
            return null;
        }
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
                Container.ChunkVoxelMaxAmtXZ,
                Container.ChunkVoxelMaxAmtXZ * 2,
                Container.ChunkVoxelMaxAmtXZ);


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
                                //Vector3 debrisDirection = (voxelPos - center).normalized + impactNormal;
                                EmitDebrisAt(TruePos, WorldManager.Instance.regions[(int)TruePos.w - 1].colour.color);
                            }
                        }
                    }
                }
            }


            if (chunkModified && !dirtyChunks.Contains(chunk))
            {
                dirtyChunks.Enqueue(chunk);
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
            StartCoroutine(SpawnPhysicsVoxelDebrisInArray(RemoveVoxelsInArea(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), explosionRadius)));
            StartCoroutine(ApplyExplosionForceWithDelay(hit.point));
        }
        else
        {
            hasHit = false;
        }
    }

    public void EmitDebrisAt(Vector3 position, Color color)
    {
        var emitParams = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = Random.insideUnitSphere * 5f,
            startLifetime = Random.Range(0.8f, 1.5f),
            startSize = Random.Range(0.8f, 1.2f),
            startColor = color
        };

        particleSystem.Emit(emitParams, Random.Range(3, 6)); // emit 3-6 particles at this position
    }
    public IEnumerator SpawnPhysicsVoxelDebrisInArray(List<Vector4> affectedVoxels)
    {
        int VoxelCount = 0;
        foreach (Vector4 voxelData in affectedVoxels)
        {
            VoxelCount++;
            Vector3 position = new Vector3(voxelData.x, voxelData.y, voxelData.z);
            int voxelID = (int)voxelData.w;

            if (voxelID > 0 && voxelID <= WorldManager.Instance.regions.Length)
            {
                SpawnVoxelDebris(position, WorldManager.Instance.regions[voxelID - 1].colour);
            }
            if (VoxelCount > 32)
            {
                VoxelCount = 0;
                yield return null;
            }
        }
    }
    //private void ApplyExplosionForce(Vector3 explosionCenter)
    //{
    //    // The only things that would be affected here would be voxel debris so just do the thing
    //    Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionRadius, physicsLayerMask);

    //    foreach (Collider hit in colliders)
    //    {
    //        if (VoxelDebris.TryGetVoxelDebris(hit.gameObject, out VoxelDebris VBscript))
    //        {

    //            VBscript._rb.AddExplosionForce(
    //                        explosionForce,
    //                        explosionCenter,
    //                        explosionRadius,
    //                        upwardModifier,
    //                        forceMode);

    //            Debug.Log("Found a voxel debirs");

    //        }
    //    }
    //}


    public VoxelDebris SpawnVoxelDebris(Vector3 position, VoxelColor voxelColor, float AngularVelocity = 5f)
    {
        if (voxelDebrisPrefab == null) return null;
        GameObject debris = ObjectPool.GetObj(voxelDebrisPrefab.name);
        debris.transform.position = position;
        debris.transform.rotation = Quaternion.identity;
        debris.layer = 7;

        if (VoxelDebris.TryGetVoxelDebris(debris, out VoxelDebris VBscript))
        {
            VBscript._renderer.material.color = voxelColor.color;
            VBscript._renderer.material.SetFloat("_Metallic", voxelColor.metallic);
            VBscript._renderer.material.SetFloat("_Glossiness", voxelColor.smoothness);
            VBscript._rb.angularVelocity = new Vector3(
            Random.Range(-AngularVelocity, AngularVelocity),
            Random.Range(-AngularVelocity, AngularVelocity),
            Random.Range(-AngularVelocity, AngularVelocity));
        }
        return VBscript;
    }


    public IEnumerator ApplyExplosionForceWithDelay(Vector3 explosionCenter, float delay = 0.05f, float explosionradius = 3.5f)
    {
        yield return new WaitForSeconds(delay);
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, explosionradius, physicsLayerMask);
        foreach (Collider hit in colliders)
        {
            if (VoxelDebris.TryGetVoxelDebris(hit.gameObject, out VoxelDebris VBscript))
            {
                VBscript._rb.AddExplosionForce(
                    explosionForce,
                    explosionCenter,
                    explosionradius,
                    upwardModifier,
                    forceMode);
                Debug.Log("Affected debris");
            }
            //Debug.Log("Affected debris");
        }
    }



    /// LATER ALL THESE GET COMPONENTS WE SHOULD USE THE STRATEGY WE LEARNT FROM MK AND REY PROJECT ( WARNING LINE STRAT)
}