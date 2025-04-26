using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class VoxelDebris : MonoBehaviour
{
    private static Dictionary<GameObject, VoxelDebris> cache = new Dictionary<GameObject, VoxelDebris>();
    public Rigidbody _rb;
    public Renderer _renderer;
    public Vector3 BasicVoxelScale;

    [SerializeField]
    private AnimationCurve _curve;
    private float timer = 0;    
    private void Awake()
    {
        cache[this.gameObject] = this;
    }
    private void OnDestroy()
    {
        cache.Remove(this.gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        transform.localScale = BasicVoxelScale;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        float scalevalue = _curve.Evaluate(timer);
        transform.localScale = BasicVoxelScale * scalevalue;
        if (scalevalue <= 0.01)
        {
            ObjectPool.ReturnObj(gameObject);
        }
    }

    public static bool TryGetVoxelDebris(GameObject obj, out VoxelDebris voxelDebris)
    {
        return cache.TryGetValue(obj, out voxelDebris);
    }
}
