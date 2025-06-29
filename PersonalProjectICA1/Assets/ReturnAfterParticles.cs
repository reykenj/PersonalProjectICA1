using System.Collections;
using UnityEngine;
public class ReturnAfterParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private void Awake()
    {
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
        }
    }

    private void OnEnable()
    {
        StartCoroutine(CheckIfDone());
    }

    private IEnumerator CheckIfDone()
    {
        yield return new WaitUntil(() => !ps.IsAlive(true));
        ObjectPool.ReturnObj(gameObject);
    }
}
