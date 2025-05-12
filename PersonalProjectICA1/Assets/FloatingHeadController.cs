using System.Collections;
using UnityEngine;

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
    [SerializeField] AttackHandler projectileAttackHandler;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
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
        transform.LookAt(PlayerTransform.position + Vector3.up);
        projectileAttackHandler.AttackStartPoint.LookAt(PlayerTransform.position + Vector3.up);
        if (SendOutAttack == null)
        {
            SendOutAttack = StartCoroutine(ProjectileAttack());
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
            if (Physics.Raycast(transform.position, (PlayerTransform.position + Vector3.up - transform.position).normalized, out RaycastHit hit, AttackRange, LayerMask.GetMask("Player")))
            {
                if(TargetTransform.parent != hit.collider.transform) TargetTransform.SetParent(hit.collider.transform);
                TargetTransform.localPosition = Vector3.up * (AttackRange - 1);
                if (Physics.Raycast(PlayerTransform.position, Vector3.up, out RaycastHit hit2, AttackRange - 2, LayerMask.GetMask("Voxel")))
                    TargetTransform.localPosition = Vector3.up * hit2.distance;
                // maybe we shoot another raycast upwards for this to determin whats the highest place the skull can go
                // without breaking line of sight from player
                //but for now lets jkust multiple this Vector3.up, nvm

                // TO DO: ADD MINIMUM RANGE, IF THE SKULL IS TOO CLOSE TO THE PLAYER OR BELOW THE PLAYER, PATHFIND FIRST TO THE TOP
                CurrState = BehaviourState.Attacking;
            }
            else
            {
                CurrState = BehaviourState.Pathing;
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }
    IEnumerator ProjectileAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            projectileAttackHandler.isMainHandler = true;
            projectileAttackHandler.Cast();
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(AttackCooldown);
        SendOutAttack = null;
    }
}
