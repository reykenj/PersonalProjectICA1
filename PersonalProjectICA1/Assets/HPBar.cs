using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Humanoid controller;
    [SerializeField] private Slider slider;      // Instant HP
    [SerializeField] private Slider lerpslider;  // Delayed HP

    [SerializeField] private float lerpSpeed = 5f;

    void Start()
    {
        if(controller == null)
        {
            controller = GameFlowManager.instance.Player.GetComponent<Humanoid>();
        }
        controller.OnHurt += OnHurt;
        OnHurt(); // Initialize on start
        lerpslider.maxValue = slider.maxValue;
        lerpslider.value = slider.value;
    }

    void Update()
    {
        if (Mathf.Abs(lerpslider.value - slider.value) > 0.01f)
        {
            lerpslider.value = Mathf.Lerp(lerpslider.value, slider.value, Time.deltaTime * lerpSpeed);
        }
        else
        {
            lerpslider.value = slider.value; // Snap to avoid jittering at low differences
        }
    }

    void OnHurt()
    {
        float maxhp = 0;
        float hp = 0;
        controller.GetHPInfo(out hp, out maxhp);

        slider.maxValue = maxhp;
        lerpslider.maxValue = maxhp;

        slider.value = hp;
        //// Only initialize lerpslider if it hasn't caught up
        //if (lerpslider.value < hp)
        //    lerpslider.value = hp;
    }
}
