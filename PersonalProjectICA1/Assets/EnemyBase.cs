using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    public bool SawPlayer = false;
    public Humanoid humanoid;
    public Transform PlayerTransform;
    public List<AttackHandler> AttackHandlers;
    public Transform TargetTransform;
    public CharacterController characterController;
    public float SightRange;
    public float AttackRange;
    public float AttackCooldown;
    public bool Tracking = true;
}
