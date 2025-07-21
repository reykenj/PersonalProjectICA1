using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public bool SawPlayer = false;
    public Humanoid humanoid;
    public Transform PlayerTransform;
    public List<AttackHandler> AttackHandlers;
    public Transform TargetTransform;
    public float TargetTransformUpMult;
    public CharacterController characterController;
    public float SightRange;
    public float AttackRange;
    public float AttackCooldown;
    public bool Tracking = true;

    protected static Dictionary<GameObject, EnemyBase> cache = new Dictionary<GameObject, EnemyBase>();

    public static bool TryGetEnemy(GameObject obj, out EnemyBase enemy)
    {
        return cache.TryGetValue(obj, out enemy);
    }
    abstract public void StartTracking();
}
