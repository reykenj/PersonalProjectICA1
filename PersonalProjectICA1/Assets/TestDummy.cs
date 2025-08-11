using TMPro;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Stats;
    [SerializeField] Humanoid humanoid;

    private float prevhp;
    private float damagePerHit = 0.0f;
    private float dps = 0.0f;
    private float timer = 0.0f;

    private void Start()
    {
        humanoid.OnHurt += OnHurt;
        humanoid.GetHPInfo(out prevhp, out float maxhp);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            dps = 0f;
        }
        Stats.text = "DPS: " + dps.ToString() + "\nDamage Per Hit: " + damagePerHit.ToString();
    }

    private void OnHurt()
    {
        humanoid.GetHPInfo(out float currentHp, out float maxhp);

        if ((prevhp - currentHp) < 0.0f)
        {
            prevhp = currentHp;
            return;
        }

        damagePerHit = prevhp - currentHp;

        dps += damagePerHit;
        if (timer <= 0f)
        {
            timer = 1.0f;
        }

        prevhp = currentHp;
    }
}
