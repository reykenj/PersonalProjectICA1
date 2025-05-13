using UnityEngine;
using UnityEngine.InputSystem;

public class DemonicMinionController : MonoBehaviour
{
    private enum State
    {
        Attacking,
        Moving,
        Idle,
        Death,
    }
    [SerializeField] Humanoid humanoid;
    [SerializeField] private Animator _animator;

    [SerializeField] AttackHandler attackHandler;
    Vector3 moveDirect = Vector3.zero;
    private State _currState;
    private int HitBodyLayerIndex;


    [SerializeField] private CharacterController characterController;
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
        HitBodyLayerIndex = _animator.GetLayerIndex("Hit");
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


        moveDirect = transform.forward;
        if (humanoid.IsRagdolled()) moveDirect = Vector3.zero;

        Vector3 moveDirection = moveDirect.normalized * humanoid.SpeedMultiplier * Time.deltaTime;
        characterController.Move(moveDirection);

        //_animator.SetFloat("MovementSpeedMult", humanoid.SpeedMultiplier);
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
            _currState = nextState;
        }
    }

    //private void OnAnimatorMove()
    //{
    //    //if (Humanoid.IsDead()) return;

    //    Vector3 animationDelta = _animator.deltaPosition;
    //    float animationMoveDistance = animationDelta.magnitude;
    //    Vector3 moveDirection = moveDirect.normalized * animationMoveDistance;
    //    characterController.Move(moveDirection);
    //}
}
