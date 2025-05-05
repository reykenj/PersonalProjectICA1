using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.Rendering;

public class Humanoid : MonoBehaviour
{
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
    private Vector3 GravityVel;
    public System.Action OnGrounded;
    public System.Action OnShieldBreak;
    public System.Action OnHurt;
    public System.Action OnDeath;

    private void Start()
    {
        HP = MaxHP;
        Shield = MaxShield;
    }
    void Update()
    {
        Vector3 finalVel = Vector3.zero;
        if (Gravity)
        {
            if (!IsGrounded())
            {
                GravityVel.y += gravity * Time.deltaTime;
            }
            else if (GravityVel.y < -1) GravityVel.y = -1;
            //else
            //{
            //    GravityVel.y = -2;
            //}
            finalVel += GravityVel;
        }
        if (HP > 0)
            _charController.Move(finalVel * Time.deltaTime);

        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out RaycastHit hit, 0.2f, LayerMask.GetMask("Voxel"))
        && GravityVel.y <= 0 && !Grounded)
        {
            Grounded = true;
            OnGrounded?.Invoke();
        }
        else
            Grounded = false;
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
        if (Shield > 0)
        {
            Shield = Mathf.Clamp(Shield - Damage, 0, MaxShield);
        }
        else
        {
            HP = Mathf.Clamp(HP - Damage, 0, MaxHP);
        }
    }
}
