using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Projectile : MonoBehaviour
{
    [SerializeField] private ProjectileInformation ProjInfo;
    [SerializeField] private MeshCollider _collider;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;
    public float duration = 1.0f;
    public Transform TargetTransform;
    public GameObject CollisionEffectPrefab;
    //public GameObject SpawnEffectPrefab;
    private Vector3 _currentRotation;
    private Vector3 _lastDirection;
    //private Coroutine lifetimecoroutine;
    public GameObject Owner;


    private static Dictionary<GameObject, Projectile> cache = new Dictionary<GameObject, Projectile>();
    private void Awake()
    {
        cache[gameObject] = this;
    }
    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }
    private void ProjInit()
    {
        _meshFilter.mesh = ProjInfo.mesh;
        _collider.sharedMesh = ProjInfo.mesh;
        _meshRenderer.enabled = ProjInfo.Render;
        SetPhysics(ProjInfo.Physics);
        transform.localScale = new Vector3(ProjInfo.ScaleCurveX.Evaluate(0), ProjInfo.ScaleCurveY.Evaluate(0), ProjInfo.ScaleCurveZ.Evaluate(0));
        //ChangeLifetime(ProjInfo.lifetime);
        ProjInfo.timer = 0;
        if (ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
        {
            _rb.isKinematic = true;
        }
    }

    public void SetProjInfo(ProjectileInformation projInfo)
    {
        ProjInfo = projInfo;
        ProjInit();
    }
    void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
        }
        _currentRotation = ProjInfo.StartingRotation;
        ProjInfo._initialPosition = transform.position;
    }
    private void OnEnable()
    {
        _collider.enabled = true;

        if (ProjInfo.OnSpawn != null)
        {
            ProjInfo.OnSpawn.Invoke(this);
        }
        ProjInit();
    }
    private void OnDisable()
    {
        _rb.isKinematic = true;

        //if (lifetimecoroutine != null)
        //{
        //    StopCoroutine(lifetimecoroutine);
        //    lifetimecoroutine = null;
        //}
        ProjectileInformation info = ProjInfo;

        if (info.OnDespawn != null)
        {
            info.OnDespawn = null;
        }
        if (info.OnCollision != null)
        {
            info.OnCollision = null;
        }
        if (info.OnSpawn != null)
        {
            info.OnSpawn = null;
        }

        ProjInfo = info;
    }
    //public void ChangeLifetime(float newLifetime)
    //{
    //    if (lifetimecoroutine != null)
    //    {
    //        StopCoroutine(lifetimecoroutine);
    //        lifetimecoroutine = null;
    //    }
    //    ProjInfo.lifetime = newLifetime;
    //    lifetimecoroutine = StartCoroutine(DisableWhenLifetimeEnds());
    //}
    //private IEnumerator DisableWhenLifetimeEnds()
    //{
    //    yield return new WaitForSeconds(ProjInfo.lifetime);

    //    if (OnDespawn != null)
    //    {
    //        OnDespawn.Invoke(this);
    //        OnDespawn = null;
    //    }

    //    ObjectPool.ReturnObj(this.gameObject);
    //}
    private void OnTriggerStay(Collider other)
    {
        if (other == null) return;
        if (other.gameObject == Owner) return;
        if (ProjInfo.DestructionRadius > 0)
        {
            List<Vector4> affectedVoxels = ChunkManager.Instance.RemoveVoxelsInArea(transform.position, ProjInfo.DestructionRadius); // Something wrong with this if we make it a child
            if (ProjInfo.CreateDebrisOnDestruction)
            {
                StartCoroutine(ChunkManager.Instance.SpawnPhysicsVoxelDebrisInArray(affectedVoxels));
            }
        }

        Humanoid.TryGetHumanoid(other.gameObject, out Humanoid hurtcontroller);
        if (CollisionEffectPrefab != null)
        {
            GameObject CollisionSpawn = ObjectPool.GetObj(CollisionEffectPrefab.name);
            CollisionSpawn.transform.position = transform.position;
        }
        if (hurtcontroller != null)
        {
            Debug.Log("Hurting! " + other.gameObject.name);
            hurtcontroller.Hurt(ProjInfo.Damage);
        }

        if (ProjInfo.OnCollision != null)
        {
            ProjInfo.OnCollision.Invoke(this);
            //ProjectileInformation info = ProjInfo;
            //info.OnCollision = null;
            //ProjInfo = info;
        }

        if (ProjInfo.ReturnOnCollision)
        {
            if (ProjInfo.OnDespawn != null)
            {
                ProjInfo.OnDespawn.Invoke(this);
            }

            ObjectPool.ReturnObj(gameObject);
            return;
        }
        else if(hurtcontroller != null)
        {
            StartCoroutine(WaitForHurtTimer());
        }
    }
    IEnumerator WaitForHurtTimer()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(ProjInfo.HurtTimer);
        if (gameObject.activeSelf)
        {
            _collider.enabled = true;
        }
    }

    private void SetPhysics(bool NewPhysics)
    {
        ProjInfo.Physics = NewPhysics;
        if (!ProjInfo.Physics)
        {
            _rb.isKinematic = true;
        }
        else
        {
            _rb.isKinematic = false;
        }
    }

    void FixedUpdate()
    {
        HandlePhysicsMovement();
        HandlePhysicsRotation();
    }

    void Update()
    {
        if (ProjInfo.timer >= ProjInfo.lifetime)
        {
            if (ProjInfo.OnDespawn != null)
            {
                ProjectileInformation info = ProjInfo;
                info.OnDespawn = null;
                ProjInfo = info;
            }
            ObjectPool.ReturnObj(gameObject);
            return;
        }
        ProjInfo.timer += Time.deltaTime;
        float t = ProjInfo.timer / duration;

        transform.localScale = new Vector3(ProjInfo.ScaleCurveX.Evaluate(t), ProjInfo.ScaleCurveY.Evaluate(t), ProjInfo.ScaleCurveZ.Evaluate(t));

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
        float beamLength = transform.localScale.z;
        transform.position = ProjInfo._initialPosition + (transform.forward * beamLength * 0.5f);
    }

    private void UpdateVisualRotation(float t)
    {
        switch (ProjInfo.rotationT)
        {

            case ProjectileInformation.RotationType.XYZAxisCurve:
                _currentRotation.x = ProjInfo.StartingRotation.x + ProjInfo.RotationCurveX.Evaluate(t);
                _currentRotation.y = ProjInfo.StartingRotation.y + ProjInfo.RotationCurveY.Evaluate(t);
                _currentRotation.z = ProjInfo.StartingRotation.z + ProjInfo.RotationCurveZ.Evaluate(t);
                transform.rotation = Quaternion.Euler(_currentRotation);
                break;

            case ProjectileInformation.RotationType.None:
                if (ProjInfo.movementT == ProjectileInformation.MovementType.BeamExtendForward)
                {
                    transform.rotation = Quaternion.Euler(_currentRotation);
                }
                break;
        }
    }

    public static bool TryGetProj(GameObject obj, out Projectile projectile)
    {
        return cache.TryGetValue(obj, out projectile);
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
        XYZAxisCurve
    }

    [Header("Movement")]
    public AnimationCurve speedCurve;
    public AnimationCurve MoveYCurve;
    public AnimationCurve MoveXCurve; 
    public MovementType movementT;
    public Vector3 _initialPosition;
    public Vector3 Direction;

    [Header("Rotation")]
    public AnimationCurve RotationCurveX;
    public AnimationCurve RotationCurveY;
    public AnimationCurve RotationCurveZ;
    public Vector3 StartingRotation;
    public RotationType rotationT;
    public float rotationSpeed;

    [Header("Scale")]
    public AnimationCurve ScaleCurveX;
    public AnimationCurve ScaleCurveY;
    public AnimationCurve ScaleCurveZ;

    [Header("Collision Effects")]
    public float Damage;
    public float HurtTimer;
    public float DestructionRadius;
    public bool ReturnOnCollision;
    public bool CreateDebrisOnDestruction;
    public bool Physics;

    [Header("Others")]
    public float timer;
    public float lifetime;
    public bool Render;
    public Mesh mesh;

    public System.Action<Projectile> OnCollision;
    public System.Action<Projectile> OnSpawn;
    public System.Action<Projectile> OnDespawn;
}