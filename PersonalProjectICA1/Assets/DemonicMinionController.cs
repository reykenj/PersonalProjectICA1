using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Rendering.InspectorCurveEditor;

public class DemonicMinionController : MonoBehaviour
{
    private enum State
    {
        Attacking,
        Moving,
        Idle,
        Death,
    }

    private enum BehaviourState
    {
        Pathing,
        Attacking
    }
    [SerializeField] Humanoid humanoid;
    [SerializeField] private Animator _animator;
    Vector3 moveDirect = Vector3.zero;
    private State CurrAnimState;
    private BehaviourState CurrState;
    private int HitBodyLayerIndex;
    [SerializeField] private CharacterController characterController;
    // ATTACKING
    [SerializeField] float SightRange;
    [SerializeField] float AttackRange;
    [SerializeField] float AttackCooldown;
    [SerializeField] Transform PlayerTransform;
    [SerializeField] List<AttackHandler> AttackHandlers;
    [SerializeField] ParticleSystem AttackVFX;

    bool SawPlayer = false;
    // PATHFINDING
    [SerializeField] Transform TargetTransform;
    [SerializeField] VoxelAStarPathing VoxelAStarPathing;
    [SerializeField] float MaxSearchTimer = 1.0f;
    [SerializeField] float PathfindingAccuracy = 0.5f;
    private int SearchCount;
    [SerializeField] private Vector3 Direction;
    Vector3 TargetPos;
    [SerializeField] Vector3 Diff;
    Coroutine FindNewPath;
    Coroutine SeePlayer;
    Coroutine SendOutAttack;
    bool Tracking = true;
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
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
        if (SendOutAttack != null)
        {
            StopCoroutine(SendOutAttack);
            SendOutAttack = null;
        }
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




    private void ChangeState(State nextState, int LayerIndex)
    {
        if (!CanTransition(LayerIndex)) return;
        if (LayerIndex == 0)
        {
            CurrAnimState = nextState;
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

                //Debug.Log("Trying to pathfind this floating head now!");
            }
            //Debug.Log("floating head");
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
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
                    //characterController.Move(Direction * humanoid.SpeedMultiplier * Time.deltaTime);
                    characterController.Move(Direction * (humanoid.SpeedMultiplier * Time.deltaTime));
                    //Debug.Log("GOING " + Direction);
                }
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
            //if (Random.Range(0, 2) == 0)
            //{
            //    SendOutAttack = StartCoroutine(ProjectileAttack());
            //}
            //else
            //{
            //    SendOutAttack = StartCoroutine(BeamAttack());
            //}
        }
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
                        TargetTransform.localPosition = Vector3.up * 0.5f;
                    }
                    SawPlayer = true;
                }
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }

    void OnDeath()
    {
        TargetTransform.SetParent(transform);
    }
}
