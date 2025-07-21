using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Rendering.InspectorCurveEditor;

public class DemonicMinionController : EnemyBase
{


    private static readonly string Attack = "Attack";
    private enum BehaviourState
    {
        Pathing,
        Attacking
    }
    [SerializeField] private Animator _animator;
    Vector3 moveDirect = Vector3.zero;
    private BehaviourState CurrState;
    private int HitBodyLayerIndex;
    // ATTACKING
    [SerializeField] ParticleSystem AttackVFX;
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
    Coroutine AttackDelay;
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }

    private void Awake()
    {
        cache[gameObject] = this;
        humanoid.OnDeath += OnDeath;
    }
    void Start()
    {
        HitBodyLayerIndex = _animator.GetLayerIndex("Hit");
    }

    private void OnEnable()
    {
        SawPlayer = false;
        if (PlayerTransform == null)
        {
            PlayerTransform = GameFlowManager.instance.Player.transform;
        }
        Tracking = true;
        Wander();
        VoxelAStarPathing.Pathfind();

        FindNewPath = StartCoroutine(FindPath());
        SeePlayer = StartCoroutine(TrySeePlayer());

    }


    void OnDisable()
    {
        if (FindNewPath != null)
        {
            StopCoroutine(FindNewPath);
            FindNewPath = null;

        }
        if (SeePlayer != null)
        {
            StopCoroutine(SeePlayer);
            SeePlayer = null;
        }

        if (AttackDelay != null)
        {
            StopCoroutine(AttackDelay);
            AttackDelay = null;
        }
        //if (SendOutAttack != null)
        //{
        //    StopCoroutine(SendOutAttack);
        //    SendOutAttack = null;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (humanoid.IsDead())
        {
            //humanoid.SetDirection(new Vector3(0, 0, 0));
            characterController.enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale <= 0.0f || humanoid.IsDead())
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
    }


    private bool CanTransition(int LayerIndex)
    {
        AnimatorStateInfo currentState =
        _animator.GetCurrentAnimatorStateInfo(LayerIndex);
        if (LayerIndex == 0)
        {
            return !_animator.IsInTransition(0);
        }
        else if (LayerIndex == HitBodyLayerIndex)
        {
            return !_animator.IsInTransition(HitBodyLayerIndex);
        }
        return false;
    }

    IEnumerator FindPath()
    {
        while (true)
        {
            float SearchTime = 0;
            if ((VoxelAStarPathing.PathFound.Count <= 0 || SawPlayer) && CurrState == BehaviourState.Pathing)
            {
                if (TargetTransform.parent == null)
                {
                    Wander();
                }
                VoxelAStarPathing.Pathfind();
            }
            if (!SawPlayer)
            {
                SearchTime += MaxSearchTimerUnseen;
            }
            else
            {
                SearchTime = Random.Range(0, MaxSearchTimerSeen);
            }
            yield return new WaitForSeconds(SearchTime);
        }
    }


    private void Wander()
    {
        if (TargetTransform.parent != null)
        {
            TargetTransform.SetParent(null); // When returning, remember to reparent this back to the floating head.
        }
        TargetTransform.position = VoxelAStarPathing.PickRandomVoxelPos(transform.position, true, 10, 5);
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
                if (humanoid.IsGrounded())
                {
                    transform.position = TargetPos;
                }
                VoxelAStarPathing.PathFound.RemoveAt(0);
            }
            else
            {
                if (Direction.y > 0.5f && humanoid.IsGrounded())
                {
                    humanoid.JumpHeight = Diff.y;
                    humanoid.Jump();
                }
                else
                {
                    characterController.Move(Direction * (humanoid.SpeedMultiplier * Time.deltaTime));
                }
                _animator.SetBool("IsPathing", true);
            }
        }
        else
        {
            _animator.SetBool("IsPathing", false);
        }
    }

    private void Attacking()
    {
        if(AttackDelay != null) { return; }

        AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(0);
        if (currentState.shortNameHash != Animator.StringToHash(Attack))
        {
            if (!_animator.IsInTransition(0))
            {
                Debug.Log("[DemonicMinion] Attacking!");
                _animator.SetTrigger("Attack");
            }

            if (Tracking)
            {
                transform.LookAt(PlayerTransform.position + Vector3.up);
                //AttackHandlers[0].AttackStartPoint.LookAt(PlayerTransform.position + Vector3.up);
            }
        }
    }

    public void AttackFinish()
    {
        AttackDelay = StartCoroutine(ContinueAttackCheck());
    }

    public void LaunchAttack()
    {
        transform.LookAt(PlayerTransform.position + Vector3.up * 2);
        AttackHandlers[0].MultiCast(AttackHandlers[0].AttackStartPoint.position, AttackHandlers[0].AttackStartPoint.rotation);
    }

    IEnumerator ContinueAttackCheck()
    {
        yield return new WaitForSeconds(AttackCooldown);
        if (Vector3.Distance(transform.position, PlayerTransform.position) <= AttackRange)
        {
            CurrState = BehaviourState.Attacking;
        }
        else
        {
            CurrState = BehaviourState.Pathing;
        }
        AttackDelay = null;
    }

    IEnumerator TrySeePlayer()
    {
        while (true)
        {
            if (!SawPlayer)
            {
                bool RayHit = Physics.Raycast(transform.position,
                (PlayerTransform.position + Vector3.up - transform.position).normalized,
                out RaycastHit hit,
                SightRange,
                LayerMask.GetMask("Player", "Voxel"));
                if (RayHit && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    if (TargetTransform.parent != hit.collider.transform)
                    {
                        TargetTransform.SetParent(hit.collider.transform);
                        TargetTransform.localPosition = Vector3.up * TargetTransformUpMult;
                    }
                    SawPlayer = true;

                    StartTracking();
                }
            }
            else if (CurrState == BehaviourState.Pathing)
            {
                if(Vector3.Distance(transform.position, PlayerTransform.position) <= AttackRange)
                {
                    CurrState = BehaviourState.Attacking;
                }
                
            }
            yield return new WaitForSeconds(1.0F + Random.Range(-0.25f, 0.25f));
        }
    }

    void OnDeath()
    {
        TargetTransform.SetParent(transform);
    }

    public override void StartTracking()
    {
        if (FindNewPath != null)
        {
            StopCoroutine(FindNewPath);
        }
        FindNewPath = StartCoroutine(FindPath());

        VoxelAStarPathing.PathFound.Clear();
    }
}
