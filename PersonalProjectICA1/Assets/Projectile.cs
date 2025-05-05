using System;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Projectile : MonoBehaviour
{
    [SerializeField] ProjectileInformation ProjInfo;
    private Rigidbody _rb;
    public float duration = 1.0f;
    public Transform TargetTransform;
    private Vector3 _currentRotation;
    private Vector3 _lastDirection;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _currentRotation = ProjInfo.StartingRotation;
        ProjInfo._initialPosition = transform.position;
    }

    private void OnEnable()
    {
        ProjInfo.timer = 0;
        if (ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
        {
            _rb.isKinematic = true;
        }
    }

    private void OnDisable()
    {
        _rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        HandlePhysicsMovement();
        HandlePhysicsRotation();
    }

    void Update()
    {
        ProjInfo.timer += Time.deltaTime;
        float t = ProjInfo.timer / duration;

        UpdateVisualRotation(t);

        if (ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
        {
            UpdateBeamPosition();
        }
    }

    private void HandlePhysicsMovement()
    {
        if (ProjInfo.movementT == ProjectileInformation.MovementType.None || ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
            return;

        float speed = ProjInfo.speedCurve.Evaluate(ProjInfo.timer / duration);
        float y = ProjInfo.MoveYCurve.Evaluate(ProjInfo.timer / duration);
        float x = ProjInfo.MoveXCurve != null ? ProjInfo.MoveXCurve.Evaluate(ProjInfo.timer / duration) : 0f;

        Vector3 velocity;
        switch (ProjInfo.movementT)
        {
            case ProjectileInformation.MovementType.MoveForward:
                ProjInfo.Direction = transform.forward;
                break;
        }

        Vector3 upMovement = Vector3.Cross(ProjInfo.Direction, Vector3.up) * y;
        Vector3 forwardMovement = Vector3.Cross(ProjInfo.Direction, Vector3.right) * x;
        velocity = (ProjInfo.Direction + upMovement + forwardMovement).normalized * speed;
        _rb.linearVelocity = velocity;
        _lastDirection = velocity.normalized;
    }

    private void HandlePhysicsRotation()
    {
        if (ProjInfo.rotationT == ProjectileInformation.RotationType.None)
            return;

        Quaternion targetRotation = Quaternion.identity;
        switch (ProjInfo.rotationT)
        {
            case ProjectileInformation.RotationType.Spin:
                _rb.MoveRotation(_rb.rotation * Quaternion.Euler(0, ProjInfo.rotationSpeed * Time.fixedDeltaTime, 0));
                break;

            case ProjectileInformation.RotationType.AimToMovement when _lastDirection != Vector3.zero:
                targetRotation = Quaternion.LookRotation(_lastDirection);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, ProjInfo.rotationSpeed * Time.fixedDeltaTime));
                break;

            case ProjectileInformation.RotationType.AimToTarget when TargetTransform != null:
                Vector3 direction = TargetTransform.position - transform.position;
                targetRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, ProjInfo.rotationSpeed * Time.fixedDeltaTime));
                break;
        }
    }

    private void UpdateBeamPosition()
    {
        float beamLength = transform.localScale.x;
        transform.position = ProjInfo._initialPosition + (transform.forward * beamLength * 0.5f);
    }

    private void UpdateVisualRotation(float t)
    {
        switch (ProjInfo.rotationT)
        {
            case ProjectileInformation.RotationType.ZAxisCurve:
                float curveAngleZ = ProjInfo.RotationCurveZ.Evaluate(t);
                _currentRotation.z = ProjInfo.StartingRotation.z + curveAngleZ;
                transform.rotation = Quaternion.Euler(_currentRotation);
                break;

            case ProjectileInformation.RotationType.XYZAxisCurve:
                _currentRotation.x = ProjInfo.StartingRotation.x + ProjInfo.RotationCurveX.Evaluate(t);
                _currentRotation.y = ProjInfo.StartingRotation.y + ProjInfo.RotationCurveY.Evaluate(t);
                _currentRotation.z = ProjInfo.StartingRotation.z + ProjInfo.RotationCurveZ.Evaluate(t);
                transform.rotation = Quaternion.Euler(_currentRotation);
                break;

            case ProjectileInformation.RotationType.None:
                if (ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
                {
                    // Ensure beam maintains its rotation smoothly
                    transform.rotation = Quaternion.Euler(_currentRotation);
                }
                break;
        }
    }
}

[Serializable]
public struct ProjectileInformation
{
    public enum MovementType
    {
        None,
        Normal,
        MoveForward,
        StationaryFloatingUpDown, // haven't been made
        BeamExtendForward // New movement type for beam attacks
    }

    public enum RotationType
    {
        None,
        Spin,
        AimToMovement,// haven't been made
        AimToTarget, // haven't been made
        SideSway, // haven't been made
        ZAxisCurve,
        XYZAxisCurve // New option for 3D rotation
    }

    [Header("Movement")]
    public AnimationCurve speedCurve; // Controls the movement speed over time
    public AnimationCurve MoveYCurve; // Controls the vertical movement over time
    public AnimationCurve MoveXCurve; // New curve for 3D movement
    public MovementType movementT;
    public Vector3 _initialPosition;
    public Vector3 Direction;

    [Header("Rotation")]
    public AnimationCurve RotationCurveX; // Separate curves for each axis
    public AnimationCurve RotationCurveY;
    public AnimationCurve RotationCurveZ;
    public Vector3 StartingRotation;
    public RotationType rotationT;
    public float rotationSpeed;

    [Header("Collision Effects")]
    public float Damage;
    public float DestructionRadius;

    [Header("Others")]
    public Transform TargetTransform;
    public float timer;


}