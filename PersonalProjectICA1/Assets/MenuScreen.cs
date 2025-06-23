using UnityEngine;
using UnityEngine.InputSystem;

public class MenuScreen : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _inputActions = _playerInput.actions;
        _inputActions["ToggleMenu"].Enable();

        _inputActions["ToggleMenu"].canceled += ctx =>
        {
            if (gameObject.activeSelf)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Time.timeScale = 0f;
                AudioListener.pause = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            gameObject.SetActive(!gameObject.activeSelf);
        };
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }
}
