using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class FloatingHeadController : MonoBehaviour
{
    private enum BehaviourState
    {
        Pathing,
        Attacking
    }

    [SerializeField] Humanoid humanoid;
    [SerializeField] CharacterController characterController;
    [SerializeField] BehaviourState CurrState;
    // ATTACKING
    [SerializeField] float AttackRange;
    [SerializeField] float AttackCooldown;
    [SerializeField] Transform PlayerTransform;
    [SerializeField] List<AttackHandler> AttackHandlers;
    [SerializeField] ParticleSystem FireVFX;
    // PATHFINDING
    [SerializeField] Transform TargetTransform;
    [SerializeField] VoxelAStarPathing VoxelAStarPathing;
    [SerializeField] float MaxSearchTimer = 1.0f;
    [SerializeField] int MaxSearchCount = 5;
    [SerializeField] float PathfindingAccuracy = 0.5f;
    private int SearchCount;
    private Vector3 Direction;
    Vector3 TargetPos;
    Vector3 Diff;
    Coroutine FindNewPath;
    Coroutine SeePlayer;
    Coroutine SendOutAttack;

    bool Tracking = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        Tracking = true;
        Wander();
        VoxelAStarPathing.Pathfind();

        FindNewPath = StartCoroutine(FindPath());
        SeePlayer = StartCoroutine(TrySeePlayer());
    }


    void OnDisable()
    {
        if (FindNewPath != null) StopCoroutine(FindNewPath);
        if (SeePlayer != null) StopCoroutine(SeePlayer);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        switch (CurrState)
        {
            case BehaviourState.Pathing:
                Pathing();
                break;
            case BehaviourState.Attacking:
                Attacking();
                break;
            default:
                break;
        }
        //Pathing();
        //characterController.Move(moveDirect.normalized * manualSpeed * Time.deltaTime * humanoid.SpeedMultiplier);
    }
    private void Wander()
    {
        if (TargetTransform.parent != null)
        {
            TargetTransform.SetParent(null); // When returning, remember to reparent this back to the floating head.
        }
        TargetTransform.position = VoxelAStarPathing.PickRandomVoxelPos(transform.position, false, 10);
        CurrState = BehaviourState.Pathing;
    }
    private void Pathing()
    {
        if (VoxelAStarPathing.PathFound.Count > 0)
        {
            transform.LookAt(TargetTransform);
            TargetPos = VoxelAStarPathing.PathFound[0] + new Vector3(0.5f, 0.5f, 0.5f);
            Diff = TargetPos - transform.position;
            Direction = Diff.normalized;
            if (Vector3.Distance(transform.position, TargetPos) < PathfindingAccuracy)
            {
                VoxelAStarPathing.PathFound.RemoveAt(0);
            }
            //characterController.Move(Direction * humanoid.SpeedMultiplier * Time.deltaTime);
            transform.position += Direction * (humanoid.SpeedMultiplier * Time.deltaTime);
            //Debug.Log("GOING " + Direction);
        }
    }
    private void Attacking()
    {
        if (Tracking)
        {
            transform.LookAt(PlayerTransform.position + Vector3.up);
            AttackHandlers[0].AttackStartPoint.LookAt(PlayerTransform.position + Vector3.up);
        }
        if (SendOutAttack == null)
        {
            if (Random.Range(0, 2) == 0)
            {
                SendOutAttack = StartCoroutine(ProjectileAttack());
            }
            else
            {
                SendOutAttack = StartCoroutine(BeamAttack());
            }
        }
    }

    IEnumerator FindPath()
    {
        while (true)
        {
            if (VoxelAStarPathing.PathFound.Count <= 0 && CurrState == BehaviourState.Pathing)
            {
                if (TargetTransform.parent == null)
                {
                    Wander();
                }
                VoxelAStarPathing.Pathfind();
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }
    IEnumerator TrySeePlayer()
    {
        while (true)
        {
            if (CurrState != BehaviourState.Attacking && 
                Physics.Raycast(transform.position, 
                (PlayerTransform.position + Vector3.up - transform.position).normalized,
                out RaycastHit hit, 
                AttackRange, 
                LayerMask.GetMask("Player")))
            {
                if (TargetTransform.parent != hit.collider.transform) TargetTransform.SetParent(hit.collider.transform);
                TargetTransform.localPosition = Vector3.up * (AttackRange - 1);
                if (Physics.Raycast(PlayerTransform.position, Vector3.up, out RaycastHit hit2, AttackRange - 2, LayerMask.GetMask("Voxel")))
                    TargetTransform.localPosition = Vector3.up * hit2.distance;
                // maybe we shoot another raycast upwards for this to determin whats the highest place the skull can go
                // without breaking line of sight from player
                //but for now lets jkust multiple this Vector3.up, nvm

                // TO DO: ADD MINIMUM RANGE, IF THE SKULL IS TOO CLOSE TO THE PLAYER OR BELOW THE PLAYER, PATHFIND FIRST TO THE TOP
                CurrState = BehaviourState.Attacking;
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }
    IEnumerator ProjectileAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            AttackHandlers[0].Cast();

            var emitParams = new ParticleSystem.EmitParams
            {
                position = AttackHandlers[0].AttackStartPoint.localPosition,
                startLifetime = Random.Range(0.8f, 1.5f),
                startSize = Random.Range(2, 4.5f),
                //startColor = color
            };

            FireVFX.Emit(emitParams, 1); // emit 3-6 particles at this position
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(AttackCooldown);
        if (!(Physics.Raycast(transform.position,
        (PlayerTransform.position + Vector3.up - transform.position).normalized,
        out RaycastHit hit,
        AttackRange,
        LayerMask.GetMask("Player"))))
        {
            CurrState = BehaviourState.Pathing;
        }
        SendOutAttack = null;
    }
    IEnumerator BeamAttack()
    {
        var emitParams = new ParticleSystem.EmitParams
        {
            position = AttackHandlers[1].AttackStartPoint.localPosition,
            startLifetime = AttackCooldown * 2,
            startSize = Random.Range(3, 5),
            startColor = UnityEngine.Color.yellow
        };
        FireVFX.Emit(emitParams, 1); // emit 3-6 particles at this position
        yield return new WaitForSeconds(AttackCooldown * 1.8f);
        Tracking = false;
        yield return new WaitForSeconds(AttackCooldown * 0.2f);
        AttackHandlers[1].Cast();
        yield return new WaitForSeconds(AttackHandlers[1].SpellArray[0].TempProjInfo.lifetime);
        Tracking = true;
        yield return new WaitForSeconds(AttackCooldown);
        SendOutAttack = null;

        if (!(Physics.Raycast(transform.position,
        (PlayerTransform.position + Vector3.up - transform.position).normalized,
        out RaycastHit hit,
        AttackRange,
        LayerMask.GetMask("Player"))))
        {
            CurrState = BehaviourState.Pathing;
        }
    }
}
