using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class CameraController : MonoBehaviour
{
    //[SerializeField] private float LookSensitivity = 10;
    //[SerializeField] private PlayerInput _input;
    //[SerializeField] private Transform fpTransform;
    //[SerializeField] private Cinemachine.CinemachineFreeLook freeLook;
    //private Cinemachine.CinemachineVirtualCamera firstPerson;
    //private Cinemachine.CinemachineBrain brain;
    //private bool switchCam = false, camBlending = false, _canTilt = true, _canRotate = true;
    //private float _camTilt = 0;
    //private bool dead = false;
    //// Start is called before the first frame update
    //void Start()
    //{
    //    firstPerson = fpTransform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
    //    brain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
    //    GetComponent<Humanoid>().OnDeath += DisableCam;
    //    Debug.Log(brain);
    //    Cursor.lockState = CursorLockMode.Locked;
    //}
    //public void DisableCam()
    //{
    //    dead = true;
    //    Cursor.lockState = CursorLockMode.Confined;
    //    freeLook.enabled = false;
    //    firstPerson.enabled = false;
    //}
    //public bool FirstPerson()
    //{
    //    return switchCam;
    //}
    //public void LockTilt(bool tilt)
    //{
    //    _canTilt = !tilt;
    //}

    //public void LockRotate(bool rotate)
    //{
    //    _canRotate = !rotate;
    //}

    //IEnumerator TransitionStart()
    //{

    //    while (brain.IsBlending) yield return null;

    //    while (brain.IsBlending) yield return null;
    //    camBlending = false;
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (camBlending || dead) return;

    //    if (_input.actions["ToggleCam"].WasPressedThisFrame())
    //    {
    //        switchCam = !switchCam;

    //        if (switchCam)
    //        {
    //            _camTilt = 0;
    //            freeLook.Priority = 10;
    //            firstPerson.Priority = 20;
    //        }
    //        else
    //        {
    //            freeLook.Priority = 20;
    //            firstPerson.Priority = 10;
    //        }
    //        camBlending = true;
    //        StartCoroutine(TransitionStart());
    //    }
    //    else if (switchCam)
    //    {
    //        Vector2 _mouseDelta = _input.actions["Look"].ReadValue<Vector2>();
    //        //Debug.Log(_mouseDelta);

    //        //Vertical camera movement
    //        //transform.rotation();
    //        if (_canRotate)
    //            transform.Rotate(transform.up, _mouseDelta.x * LookSensitivity * Time.deltaTime);
    //        if (_canTilt)
    //        {

    //            _camTilt -= _mouseDelta.y * LookSensitivity * Time.deltaTime;
    //            _camTilt = Mathf.Clamp(_camTilt, -75, 75);
    //        }
    //        else
    //            _camTilt = Mathf.Lerp(_camTilt, 0, Time.deltaTime * 10);

    //        fpTransform.localRotation = Quaternion.Euler(new Vector3(_camTilt, 0, 0));
    //    }
    //}
}
