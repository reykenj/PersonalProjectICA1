using UnityEngine;

public class RecoilManager : MonoBehaviour
{
    // Rotations
    private Vector3 currentRotation;
    public Vector3 targetRotation;

    // Settings
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Start()
    {
    }

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    //public void RecoilFire()
    //{
    //    targetRotation = new Vector3(
    //        Random.Range(-recoilX, recoilX),
    //        Random.Range(-recoilY, recoilY),
    //        Random.Range(-recoilZ, recoilZ)
    //    );
    //}

    public void ApplyRecoil(Vector3 Recoil)
    {
        targetRotation += new Vector3(
            Recoil.x,
            Random.Range(Recoil.y, Recoil.y),
            Random.Range(Recoil.z, Recoil.z)
        );
    }
}