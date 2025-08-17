using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DashLockOnAnim : MonoBehaviour
{
    [SerializeField] private float Duration = 1f;
    [SerializeField] private Vector3 startRotation = Vector3.zero;
    [SerializeField] private Vector3 endRotation = new Vector3(0, 0, 270);

    [SerializeField] private Vector3 startScale = Vector3.one;
    [SerializeField] private Vector3 endScale = new Vector3(0.25f, 0.25f, 0.25f);


    [SerializeField] private Image image;
    private Coroutine animCoroutine;
    private Camera maincam;

    private void Awake()
    {
        //image = GetComponent<Image>();
        maincam = Camera.main;
    }

    private void OnEnable()
    {
        image.transform.localEulerAngles = startRotation;
        transform.localScale = startScale;

        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(Animate());
    }

    private void OnDisable()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);
        animCoroutine = null;
    }
    private void Update()
    {
        if (maincam != null)
        {
            Vector3 Direction = Vector3.Normalize(transform.position - maincam.transform.position);
            transform.forward = Direction;
        }
    }

    private IEnumerator Animate()
    {
        float elapsedTime = 0f;
        Quaternion fromRot = Quaternion.Euler(startRotation);
        Quaternion toRot = Quaternion.Euler(endRotation);
        Color color = image.color;
        color.a = 0f;
        image.color = color;
        while (elapsedTime < Duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / Duration;
            image.transform.localRotation = Quaternion.Slerp(fromRot, toRot, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            color.a = Mathf.Lerp(0f, 1f, t);
            image.color = color;
            yield return null;
        }
        image.transform.localRotation = toRot;
        transform.localScale = endScale;
        color.a = 1f;
        image.color = color;
        animCoroutine = null;
    }
}
