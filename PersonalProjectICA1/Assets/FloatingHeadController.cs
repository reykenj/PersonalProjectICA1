using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

public class FloatingHeadController : EnemyBase
{
    private enum BehaviourState
    {
        Pathing,
        Attacking
    }
    [SerializeField] BehaviourState CurrState;
    // ATTACKING
    [SerializeField] ParticleSystem FireVFX;
    // PATHFINDING
    [SerializeField] VoxelAStarPathing VoxelAStarPathing;
    [SerializeField] float MaxSearchTimerSeen = 1.0f;
    [SerializeField] float MaxSearchTimerUnseen = 10.0f;
    [SerializeField] float PathfindingAccuracy = 0.5f;
    private int SearchCount;
    [SerializeField] private Vector3 Direction;
    Vector3 TargetPos;
    [SerializeField] Vector3 Diff;
    Coroutine FindNewPath;
    Coroutine SeePlayer;
    Coroutine SendOutAttack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        humanoid.OnDeath += OnDeath;

        if (PlayerTransform == null)
        {
            PlayerTransform = GameFlowManager.instance.Player.transform;
        }
    }
    private void OnEnable()
    {
        SawPlayer = false;
        Tracking = true;
        Wander();
        VoxelAStarPathing.Pathfind();

        FindNewPath = StartCoroutine(FindPath());
        SeePlayer = StartCoroutine(TrySeePlayer());
        
    }


    void OnDisable()
    {
        if (FindNewPath != null) { 
            StopCoroutine(FindNewPath);
            FindNewPath = null;

        }
        if (SeePlayer != null) { 
            StopCoroutine(SeePlayer);
            SeePlayer = null;
        }
        if(SendOutAttack != null) { 
            StopCoroutine(SendOutAttack);
            SendOutAttack = null;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale <= 0.0f)
        {
            return;
        }
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
                transform.position = TargetPos;
            }
            else
            {
                //characterController.Move(Direction * humanoid.SpeedMultiplier * Time.deltaTime);
                transform.position += Direction * (humanoid.SpeedMultiplier * Time.deltaTime);
                //Debug.Log("GOING " + Direction);
            }
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
            float SearchTime = Random.Range(-0.25f, 0.25f);
            if (VoxelAStarPathing.PathFound.Count <= 0 && CurrState == BehaviourState.Pathing)
            {
                if (TargetTransform.parent == null)
                {
                    Wander();
                }
                VoxelAStarPathing.Pathfind();

                //Debug.Log("Trying to pathfind this floating head now!");
            }
            if (!SawPlayer)
            {
                SearchTime += MaxSearchTimerUnseen;
            }
            else
            {
                SearchTime += MaxSearchTimerSeen;
            }
            //Debug.Log("floating head");
            yield return new WaitForSeconds(SearchTime);
        }
    }
    IEnumerator TrySeePlayer()
    {
        while (true)
        {
            if (CurrState != BehaviourState.Attacking)
            {
                bool RayHit = Physics.Raycast(transform.position,
                (PlayerTransform.position + Vector3.up - transform.position).normalized,
                out RaycastHit hit,
                AttackRange,
                LayerMask.GetMask("Player", "Voxel"));
                if (RayHit && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    //Debug.Log("[Test] DETECTED");

                    if (TargetTransform.parent != hit.collider.transform)
                    {
                        TargetTransform.SetParent(hit.collider.transform);
                        SawPlayer = true;
                        if (FindNewPath != null)
                        {
                            StopCoroutine(FindNewPath);
                            FindNewPath = StartCoroutine(FindPath());
                        }
                    }
                    // maybe we shoot another raycast upwards for this to determin whats the highest place the skull can go
                    // without breaking line of sight from player
                    //but for now lets jkust multiple this Vector3.up, nvm

                    // TO DO: ADD MINIMUM RANGE, IF THE SKULL IS TOO CLOSE TO THE PLAYER OR BELOW THE PLAYER, PATHFIND FIRST TO THE TOP
                    CurrState = BehaviourState.Attacking;
                }
            }

            if(TargetTransform.parent != transform)
            {
                TargetTransform.localPosition = Vector3.up * (AttackRange - 1);

                if (Physics.Raycast(PlayerTransform.position, Vector3.up, out RaycastHit hit2, AttackRange - 2, LayerMask.GetMask("Voxel")))
                {
                    //TargetTransform.localPosition
                    Vector3 NewPos = Vector3.up * hit2.distance;
                    NewPos.y -= 1;
                    TargetTransform.localPosition = NewPos;
                }
            }
            yield return new WaitForSeconds(1.0F + Random.Range(-0.25f, 0.25f));
        }
    }
    IEnumerator ProjectileAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            AttackHandlers[0].MultiCast(AttackHandlers[0].AttackStartPoint.position, AttackHandlers[0].AttackStartPoint.rotation);

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
        AttackHandlers[1].MultiCast(AttackHandlers[1].AttackStartPoint.position, AttackHandlers[1].AttackStartPoint.rotation);
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

    void OnDeath()
    {
        TargetTransform.SetParent(transform);
    }
}
