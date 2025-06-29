using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Humanoid : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshRenderers;
    [SerializeField] private List<SkinnedMeshRenderer> SkinnedmeshRenderers;
    [SerializeField] private CharacterController _charController;
    [SerializeField] float HP;
    [SerializeField] float MaxHP;
    [SerializeField] float Shield; // IF SHIELD IS DOWN, ANY RAGDOLL MAKING MOVES WILL RAGDOLL THE UNFORTUNATE SOUL
    [SerializeField] float MaxShield;
    [SerializeField] float speed;


    public float SpeedMultiplier;


    [SerializeField] bool Gravity;
    [SerializeField] bool Grounded;
    [SerializeField] bool Ragdolled;


    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float JumpHeight = 1.0f;
    [SerializeField] private float flashTimer = 0.0f;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private float flashIntensity = 5.0f;

    private Vector3 GravityVel;

    public Vector3 ExternalVel;
    public float externalVelDamp = 5f;

    public System.Action OnGrounded;
    public System.Action OnShieldBreak;
    public System.Action OnHurt;
    public System.Action OnDeath;

    [SerializeField] bool OneMeshMultipleMat;
    private Coroutine FlashCoroutine;

    private List<Color> originalColors = new List<Color>();
    private static Dictionary<GameObject, Humanoid> cache = new Dictionary<GameObject, Humanoid>();

    public static bool TryGetHumanoid(GameObject obj, out Humanoid humanoid)
    {
        return cache.TryGetValue(obj, out humanoid);
    }
    private void Awake()
    {
        cache[gameObject] = this;
    }
    private void OnDestroy()
    {
        cache.Remove(gameObject);
    }

    private void Start()
    {
        SetOGColors();
        HP = MaxHP;
        Shield = MaxShield;
    }
    void FixedUpdate()
    {
        Vector3 finalVel = Vector3.zero;

        if (Gravity)
        {
            if (!IsGrounded())
            {
                GravityVel.y += gravity * Time.deltaTime;
            }
            else if (GravityVel.y < -1)
            {
                GravityVel.y = -1;
            }
            finalVel += GravityVel;
        }

        finalVel += ExternalVel;

        if (HP > 0)
            _charController.Move(finalVel * Time.deltaTime);

        // Raycast ground detection
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.2f, LayerMask.GetMask("Voxel"))
        && GravityVel.y <= 0 && !Grounded)
        {
            Grounded = true;
            OnGrounded?.Invoke();
        }
        else
            Grounded = false;

        ExternalVel = Vector3.Lerp(ExternalVel, Vector3.zero, Time.fixedDeltaTime * externalVelDamp);
    }

    IEnumerator SetRagdolled(float Timer)
    {
        if (Shield > 0)
        {
            Ragdolled = true;
            yield return new WaitForSeconds(Timer);
            Ragdolled = false;
        }
    }

    public IEnumerator DashToTarget(Transform target, float dashTime = 0.3f)
    {
        Gravity = false; 
        Vector3 targetForward = target.forward;
        Vector3 targetPosition = target.position;
        float stopDistance = 1.0f;
        Vector3 dashEndPos = targetPosition - targetForward * stopDistance;
        Vector3 startPos = transform.position;
        float timer = 0;
        while (timer < dashTime)
        {
            timer += Time.deltaTime;
            float t = timer / dashTime;
            Vector3 desiredPos = Vector3.Lerp(startPos, dashEndPos, t);
            Vector3 moveDelta = desiredPos - transform.position;
            _charController.Move(moveDelta);
            yield return null;
        }
        Vector3 finalDelta = dashEndPos - transform.position;
        _charController.Move(finalDelta);
        Gravity = true;
    }



    public bool IsDead()
    {
        return HP <= 0;
    }
    public bool IsGrounded()
    {
        return Grounded || _charController.isGrounded;
    }
    public bool IsRagdolled()
    {
        return Ragdolled;
    }
    public void Jump()
    {
        GravityVel.y = Mathf.Sqrt(-2 * gravity * JumpHeight);
    }
    public void Hurt(float Damage)
    {
        if(IsDead()) return;
        if (Shield > 0)
        {
            Shield = Mathf.Clamp(Shield - Damage, 0, MaxShield);
        }
        else
        {
            HP = Mathf.Clamp(HP - Damage, 0, MaxHP);
        }

        if (meshRenderers.Count > 0 || SkinnedmeshRenderers.Count > 0)
        {
            flashTimer = 0.25f;
            if (FlashCoroutine == null)
            {
                FlashCoroutine = StartCoroutine(FlashEffect());
            }
        }
    }
    IEnumerator FlashEffect()
    {
        float timer = flashDuration;
        while (timer > 0)
        {
            float t = timer / flashDuration;
            Color flashColor = Color.white * t * flashIntensity;

            if (meshRenderers.Count > 0)
            {
                if (!OneMeshMultipleMat)
                {
                    for (int i = 0; i < meshRenderers.Count; i++)
                    {
                        meshRenderers[i].material.color = flashColor;
                    }
                }
                else
                {
                    for (int i = 0; i < originalColors.Count; i++)
                    {
                        meshRenderers[0].materials[i].color = flashColor;
                    }
                }
            }
            else
            {
                for (int i = 0; i < SkinnedmeshRenderers.Count; i++)
                {
                    SkinnedmeshRenderers[i].material.color = flashColor;
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }
        if (meshRenderers.Count > 0)
        {
            if (!OneMeshMultipleMat)
            {
                for (int i = 0; i < meshRenderers.Count; i++)
                {
                    meshRenderers[i].material.color = originalColors[i];
                }
            }
            else
            {
                for (int i = 0; i < originalColors.Count; i++)
                {
                    meshRenderers[0].materials[i].color = originalColors[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < SkinnedmeshRenderers.Count; i++)
            {
                SkinnedmeshRenderers[i].material.color = originalColors[i];
            }
        }

        FlashCoroutine = null;
    }

    void SetOGColors()
    {
        if (originalColors.Count > 0)
        {
            originalColors.Clear();
        }
        // Store original colors
        if (meshRenderers.Count > 0)
        {
            if (!OneMeshMultipleMat)
            {
                foreach (var mr in meshRenderers)
                {
                    originalColors.Add(mr.material.color);
                }
            }
            else // may change this later
            {
                foreach (var material in meshRenderers[0].materials)
                {
                    originalColors.Add(material.color);
                }
            }
        }
        else if (SkinnedmeshRenderers.Count > 0)
        {
            foreach (var smr in SkinnedmeshRenderers)
            {
                originalColors.Add(smr.material.color);
            }
        }
    }
}
