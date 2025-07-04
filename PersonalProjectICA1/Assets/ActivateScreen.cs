using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateScreen : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    private InputActionAsset _inputActions;
    [SerializeField] string Action;
    [SerializeField] bool StopTime;
    [SerializeField] List<GameObject> EnableOnActivate;
    [SerializeField] List<GameObject> DisableOnActivate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _inputActions = _playerInput.actions;
        _inputActions[Action].Enable();
        _inputActions[Action].canceled += CTXOffOn;
        OffOn();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    }


    private void OnDestroy()
    {
        _inputActions[Action].canceled -= CTXOffOn;
    }


    void CTXOffOn(InputAction.CallbackContext context)
    {
        OffOn();
    }
    void OffOn()
    {
        if (gameObject.activeSelf)
        {
            if (StopTime)
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (StopTime)
            {
                Time.timeScale = 0f;
                AudioListener.pause = true;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            for (int i = 0; i < EnableOnActivate.Count; i++)
            {
                EnableOnActivate[i].SetActive(true);
            }
            for (int i = 0; i < DisableOnActivate.Count; i++)
            {
                DisableOnActivate[i].SetActive(false);
            }
        }
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
