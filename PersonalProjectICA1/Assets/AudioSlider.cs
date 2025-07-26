using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "MasterVolume";

    void Start()
    {
        float currentValue;
        if (audioMixer.GetFloat(exposedParameter, out currentValue))
        {
            slider.value = currentValue;
        }
    }

    public void ChangeVolMixer(float value)
    {

        audioMixer.SetFloat(exposedParameter, value);
    }
}
