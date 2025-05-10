using System.Collections.Generic;
using UnityEngine;

public class VoxelDestroyOnCollide : MonoBehaviour
{
    public float destructionRadius = 0.5f;
    public bool showGizmos = true;
    public Color gizmoColor = Color.red;

    private void OnCollisionEnter(Collision collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ProcessCollision(collision);
    }

    private void ProcessCollision(Collision collision)
    {
        // Get all contact points
        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 hitPoint = contact.point;
            ChunkManager.Instance.RemoveVoxelsInArea(hitPoint, destructionRadius);
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, destructionRadius);
    }
}