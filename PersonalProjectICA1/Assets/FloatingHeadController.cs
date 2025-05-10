using System.Collections;
using UnityEngine;

public class FloatingHeadController : MonoBehaviour
{
    private enum BehaviourState
    {
        Pathing,
        Attacking
    }
    [SerializeField] AttackHandler attackHandler;
    [SerializeField] Humanoid humanoid;
    [SerializeField] CharacterController characterController;
    [SerializeField] BehaviourState CurrState;
    // ATTACKING
    [SerializeField] float AttackRange;
    [SerializeField] float AttackCooldown;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(FindPath());
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
            TargetPos = VoxelAStarPathing.PathFound[0] + new Vector3(0.5f, 0.5f, 0.5f);
            Diff = TargetPos - transform.position;
            Direction = Diff.normalized;
            if (Vector3.Distance(transform.position, TargetPos) < PathfindingAccuracy)
            {
                VoxelAStarPathing.PathFound.RemoveAt(0);
                SearchCount = 0;
            }
            //characterController.Move(Direction * humanoid.SpeedMultiplier * Time.deltaTime);
            transform.position += Direction * (humanoid.SpeedMultiplier * Time.deltaTime);
            //Debug.Log("GOING " + Direction);
        }
        //    //    if (Physics.Raycast(transform.position, Direction, out RaycastHit hit, AttackRange, LayerMask.GetMask("Player")))
        //    //    {
        //    //        TargetTransform.SetParent(hit.collider.transform);
        //    //        TargetTransform.localPosition = Vector3.up;
        //    //        CurrState = BehaviourState.Attacking;
        //    //    }
    }
    private void Attacking()
    {
        //Debug.Log("Attacking");
    }

    IEnumerator FindPath()
    {
        while (true)
        {
            if (VoxelAStarPathing.PathFound.Count <= 0)
            {
                Wander();
                VoxelAStarPathing.Pathfind();
            }
            yield return new WaitForSeconds(MaxSearchTimer + Random.Range(-0.25f, 0.25f));
        }
    }
}
