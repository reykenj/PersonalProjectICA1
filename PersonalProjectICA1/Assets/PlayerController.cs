using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum State
    {
        JumpingUp,
        JumpingDown,
        FallingIdle,
        CrouchIdle,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        M1Attack,
        M2Attack,
        Block,
        DefaultAttackingStance,
        HitLeft
    }

    private static readonly string Punch = "Punching";

    private static readonly string Skill1 = "ConsumeCards";
    private static readonly string Skill2 = "GamblerSkill2";
    private static readonly string Skill3 = "GamblerSkill3";
    private static readonly string Skill4 = "Flying";

    private InputActionAsset _inputActions;
    [SerializeField] Humanoid humanoid;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Animator _animator;
    [SerializeField] float isBusyTimer;
    [SerializeField] float isBusyMaxTimer;
    //[SerializeField] Transform bodyTarget;
    [SerializeField] float maxDistance = 0.1f;
    [SerializeField] float maxAngle = 90f;

    [SerializeField] AttackHandler LeftAttackHandler;
    [SerializeField] AttackHandler RightAttackHandler;
    Vector3 moveDirect = Vector3.zero;
    private State _currState;
    private int upperBodyLayerIndex;
    private int HitBodyLayerIndex;


    [SerializeField] private CharacterController characterController;
    Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;   
        _inputActions = _playerInput.actions;
        upperBodyLayerIndex = _animator.GetLayerIndex("UpperBody");
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


        Vector2 input = _inputActions["Move"].ReadValue<Vector2>();
        moveDirect = Vector2.zero;

        moveDirect += new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z) * input.y;
        moveDirect += mainCamera.transform.right * input.x;
        if (humanoid.IsRagdolled()) moveDirect = Vector3.zero;
        if (input.y >= 0)
        {
            _animator.SetBool("mirror", false);
            if (input.y != 0 || input.x != 0)
            {
                _animator.SetFloat("MovementSpeedMult", humanoid.SpeedMultiplier);
            }
        }
        else
        {
            _animator.SetBool("mirror", true);
            _animator.SetFloat("MovementSpeedMult", -humanoid.SpeedMultiplier);
        }


        Vector3 objectForward = transform.forward;
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraPosition = mainCamera.transform.position;

        Vector3 targetPosition;
        targetPosition = transform.position + cameraForward * maxDistance;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;


        //float angle = Vector3.Angle(objectForward, directionToTarget);

        //if (angle > maxAngle)
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Adjust rotation speed as needed
        //    transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));
        //}

        //bodyTarget.position = new Vector3(targetPosition.x, bodyTarget.position.y, targetPosition.z);


        if (input.magnitude > 0)
        {

            if (_animator.GetBool("IsFalling"))
            {
                float manualSpeed = 4.5f;
                //if (_inputActions["Sprint"].IsPressed())
                //    manualSpeed = 4.5f;
                //else
                //    manualSpeed = 1.7f;
                characterController.Move(moveDirect.normalized * manualSpeed * Time.deltaTime * humanoid.SpeedMultiplier);

                _animator.SetBool("IsRunning", false);
            }

            else if (humanoid.IsGrounded())
            {
                //_animator.SetBool("IsRunning", false);
                //if (!_inputActions["Sprint"].IsPressed())
                //{
                //    _animator.SetBool("IsWalkingForward", true);

                //}
                //else
                //{
                //    _animator.SetBool("IsRunning", true);
                //}
                _animator.SetBool("IsRunning", true);
            }
            Quaternion targetRotation = Quaternion.LookRotation(moveDirect);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        }
        else
        {
            _animator.SetBool("IsRunning", false);
        }


        if (_inputActions["M1"].IsPressed() && !
        _animator.IsInTransition(upperBodyLayerIndex))
        {
            ChangeState(State.M1Attack, upperBodyLayerIndex);
            isBusyTimer = isBusyMaxTimer;
        }
        else if (_inputActions["M2"].IsPressed() && !
        _animator.IsInTransition(upperBodyLayerIndex))
        {
            ChangeState(State.M2Attack, upperBodyLayerIndex);
            isBusyTimer = isBusyMaxTimer;
        }
        else if(isBusyTimer > 0)
        {
            isBusyTimer -= Time.deltaTime;
            _animator.SetLayerWeight(upperBodyLayerIndex, isBusyTimer);
        }

        if (_inputActions["Jump"].IsPressed() && CanTransition(0) && humanoid.IsGrounded())
        {
            humanoid.Jump();
            _animator.SetBool("IsFalling", true);
        }
        else if (humanoid.IsGrounded())
        {
            _animator.SetBool("IsFalling", false);
        }
    }


    private bool CanTransition(int LayerIndex)
    {
        AnimatorStateInfo currentState =
        _animator.GetCurrentAnimatorStateInfo(LayerIndex);
        if (LayerIndex == 0)
        {
            if (currentState.shortNameHash == Animator.StringToHash(Skill4))
            {
                return currentState.normalizedTime >= 1.0f && !
                _animator.IsInTransition(0);
            }
            else
            {
                return !_animator.IsInTransition(0);
            }
        }
        else if (LayerIndex == upperBodyLayerIndex)
        {
            for (int letter = 0; letter < 2; letter++)
            {
                for (int number = 0; number < 2; number++)
                {
                    char Letter;
                    if (letter == 0)
                    {
                        Letter = 'L';
                    }
                    else
                    {
                        Letter = 'R';
                    }
                    if (currentState.shortNameHash == Animator.StringToHash(Letter + Punch + number.ToString()))
                    {
                        bool Result = currentState.normalizedTime >= 1.0f && !
                        _animator.IsInTransition(upperBodyLayerIndex);
                        return Result;
                    }
                }
            }
            if (currentState.shortNameHash == Animator.StringToHash(Skill1))
            {
                bool Result = currentState.normalizedTime >= 1.0f && !
                _animator.IsInTransition(upperBodyLayerIndex);
                return Result;
            }
            else if (currentState.shortNameHash == Animator.StringToHash(Skill2))
            {
                bool Result = currentState.normalizedTime >= 1.0f && !
                _animator.IsInTransition(upperBodyLayerIndex);
                return Result;
            }
            else if (currentState.shortNameHash == Animator.StringToHash(Skill3))
            {
                bool Result = currentState.normalizedTime >= 1.0f && !
                _animator.IsInTransition(upperBodyLayerIndex);
                return Result;
            }
            else
            {
                return !_animator.IsInTransition(upperBodyLayerIndex);
            }
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
        switch (nextState)
        {
            case State.M1Attack:
                _animator.SetLayerWeight(upperBodyLayerIndex, 1);
                _animator.CrossFadeInFixedTime("L" + Punch + Random.Range(0,2).ToString(), 0.2f, upperBodyLayerIndex);
                break;
            case State.M2Attack:
                _animator.SetLayerWeight(upperBodyLayerIndex, 1);
                _animator.CrossFadeInFixedTime("R" + Punch + Random.Range(0, 2).ToString(), 0.2f, upperBodyLayerIndex);
                break;
            case State.HitLeft:
                _animator.CrossFadeInFixedTime("Hit Left", 0.2f, HitBodyLayerIndex);
                Debug.Log("A1c");
                break;

            case State.Skill1:
                _animator.SetLayerWeight(upperBodyLayerIndex, 1);
                _animator.CrossFadeInFixedTime(Skill1, 0.2f, upperBodyLayerIndex);
                break;
            case State.Skill2:
                _animator.SetLayerWeight(upperBodyLayerIndex, 1);
                _animator.CrossFadeInFixedTime(Skill2, 0.2f, upperBodyLayerIndex);
                break;
            case State.Skill3:
                _animator.SetLayerWeight(upperBodyLayerIndex, 1);
                _animator.CrossFadeInFixedTime(Skill3, 0.2f, upperBodyLayerIndex);
                break;
            case State.Skill4:
                _animator.SetLayerWeight(upperBodyLayerIndex, 0);
                _animator.CrossFadeInFixedTime(Skill4, 0.01f, 0);
                break;
        }
    }

    private void OnAnimatorMove()
    {
        //if (Humanoid.IsDead()) return;

        Vector3 animationDelta = _animator.deltaPosition;
        float animationMoveDistance = animationDelta.magnitude;
        Vector3 moveDirection = moveDirect.normalized * animationMoveDistance;
        characterController.Move(moveDirection);
    }


    public void CastLeftFist()
    {
        Debug.Log("CastLeft");
        LeftAttackHandler.Cast();
    }

    public void CastRightFist()
    {
        Debug.Log("CastRight");
        RightAttackHandler.Cast();
    }

}
