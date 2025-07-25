using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ReturnAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    protected static Dictionary<GameObject, ReturnAudio> cache = new Dictionary<GameObject, ReturnAudio>();

    private void Awake()
    {
        cache.Add(gameObject, this);
    }
    public static bool TryGetAudio(GameObject obj, out ReturnAudio audio)
    {
        return cache.TryGetValue(obj, out audio);
    }

    //public void SetAudio(AudioClip audio)
    //{
    //    audioSource.clip = audio;
    //}

    public void SetAudio(AudioResource audio)
    {
        audioSource.resource = audio;
    }
    void FixedUpdate()
    {
        if (!audioSource.isPlaying)
        {
            ObjectPool.ReturnObj(gameObject);
        }
    }
}
