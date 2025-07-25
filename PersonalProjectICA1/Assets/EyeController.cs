using System.Collections;
using UnityEngine;

public class EyeController : EnemyBase
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

    public int maxHelpers = 3;

    public float CallRange = 15.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }
    protected override void Awake()
    {
        base.Awake();
        cache[gameObject] = this;

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


    protected override void OnDisable()
    {
        base.OnDisable();
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
                Debug.Log("[EYEATTACK1] CAST");
                SendOutAttack = StartCoroutine(ProjectileAttack());
            }
            else
            {
                Debug.Log("[EYEATTACK2] CAST");
                SendOutAttack = StartCoroutine(RequestingBackup());
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

            if (TargetTransform.parent != transform)
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
            FireVFX.Emit(emitParams, 1);
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
    IEnumerator RequestingBackup()
    {
        var emitParams = new ParticleSystem.EmitParams
        {
            position = AttackHandlers[0].AttackStartPoint.localPosition,
            startLifetime = AttackCooldown * 2,
            startSize = Random.Range(3, 5),
            startColor = UnityEngine.Color.red
        };
        FireVFX.Emit(emitParams, 1);
        yield return new WaitForSeconds(AttackCooldown * 1.8f);

        Physics.Raycast(transform.position,
        Vector3.down, out RaycastHit hit2, 50.0f, LayerMask.GetMask("Voxel"));

        ChunkManager.Instance.EmitSonarAt(transform.position, Color.red);
        SendOutAttack = null;

        if (!(Physics.Raycast(transform.position,
        (PlayerTransform.position + Vector3.up - transform.position).normalized,
        out RaycastHit hit,
        AttackRange,
        LayerMask.GetMask("Player"))))
        {
            CurrState = BehaviourState.Pathing;
        }



        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, CallRange, LayerMask.GetMask("Enemy"));
        int helpersCalled = 0;
        foreach (var collider in nearbyColliders)
        {
            if (helpersCalled >= maxHelpers) break;
            TryGetEnemy(collider.gameObject, out EnemyBase enemy);
            if (enemy != null)
            {
                if (enemy != this && !enemy.SawPlayer)
                {
                    enemy.SawPlayer = true;
                    enemy.TargetTransform.SetParent(PlayerTransform);
                    enemy.TargetTransform.localPosition = Vector3.up * enemy.TargetTransformUpMult;

                    enemy.StartTracking();
                    helpersCalled++;
                }
            }

        }


    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    public override void StartTracking()
    {
        if (FindNewPath != null)
        {
            StopCoroutine(FindNewPath);
        }
        FindNewPath = StartCoroutine(FindPath());
    }
}
