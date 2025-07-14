using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeAnim : MonoBehaviour
{
    [SerializeField] Image image;

    public IEnumerator Fade(float maxTime, float a)
    {
        float timer = 0;
        Color oldColor = image.color;
        Color newColor = image.color;
        newColor.a = a;
        while (timer < maxTime)
        {
            timer += Time.deltaTime;
            image.color = Color.Lerp(oldColor, newColor, timer / maxTime);
            yield return null;
        }
    }
}
