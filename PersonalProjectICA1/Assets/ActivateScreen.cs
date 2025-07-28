using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    [SerializeField] List<GameObject> EnableOnDeActivate;
    [SerializeField] List<GameObject> DisableOnDeActivate;

    [SerializeField] bool EnterScreenWithNoKeyPressInit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if(_playerInput == null)
        {
            _playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();
        }
        _inputActions = _playerInput.actions;
        _inputActions[Action].Enable();
        if (!EnterScreenWithNoKeyPressInit)
        {
            _inputActions[Action].canceled += CTXOffOn;
        }
        OffOn();
    }

    
    // Update is called once per frame
    void FixedUpdate()
    {
    }


    private void OnDestroy()
    {
        if (!EnterScreenWithNoKeyPressInit)
        {
            _inputActions[Action].canceled -= CTXOffOn;
        }
    }

    IEnumerator WaitForFirstRelease()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        yield return new WaitWhile(() => _inputActions[Action].IsPressed());
        _inputActions[Action].canceled += CTXOffOn;
    }
    private void OnEnable()
    {
        if (EnterScreenWithNoKeyPressInit)
        {
            StartCoroutine(WaitForFirstRelease());
            OffOn(true);
        }
    }
    private void OnDisable()
    {
        if (EnterScreenWithNoKeyPressInit && _inputActions.FindAction(Action) != null)
        {
            _inputActions[Action].canceled -= CTXOffOn;
        }
        if (EnableOnDeActivate.Count != 0)
        {
            for (int i = 0; i < EnableOnDeActivate.Count; i++)
            {
                EnableOnDeActivate[i].SetActive(true);
            }
        }
        if (DisableOnDeActivate.Count != 0)
        {
            for (int i = 0; i < DisableOnActivate.Count; i++)
            {
                DisableOnDeActivate[i].SetActive(false);
            }
        }
    }

    void CTXOffOn(InputAction.CallbackContext context)
    {
        OffOn();
    }
    void OffOn(bool Apply = false)
    {
        if (!Apply)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
        if (!gameObject.activeSelf)
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
    }
}
