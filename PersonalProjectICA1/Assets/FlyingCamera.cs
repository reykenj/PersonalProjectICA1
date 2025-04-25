using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2f;
    public float lookSensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("Key Bindings")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode upKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.LeftControl;
    public KeyCode sprintKey = KeyCode.LeftShift;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        float currentSpeed = moveSpeed;
        if (Input.GetKey(sprintKey))
            currentSpeed *= sprintMultiplier;
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(forwardKey))
            moveDirection += transform.forward;
        if (Input.GetKey(backwardKey))
            moveDirection -= transform.forward;
        if (Input.GetKey(rightKey))
            moveDirection += transform.right;
        if (Input.GetKey(leftKey))
            moveDirection -= transform.right;
        if (Input.GetKey(upKey))
            moveDirection += Vector3.up;
        if (Input.GetKey(downKey))
            moveDirection -= Vector3.up;

        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}