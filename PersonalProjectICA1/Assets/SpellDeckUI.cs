using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellDeckUI : MonoBehaviour
{
    [SerializeField] AttackHandler attackHandler;
    [SerializeField] GameObject SpellSlotUIPrefab;


    [SerializeField] float amplitude = 15f;
    [SerializeField] float speed = 2f;
    [SerializeField] float activeDuration = 0.4f;
    [SerializeField] float delayBetweenSlots = 0.12f;
    Coroutine WaveAnim;

    private void Awake()
    {
        OnSpellDeckChangeSize();

        RefreshAffected();
    }
    private void OnEnable()
    {
        RefreshUISpellDeck();
        WaveAnim = StartCoroutine(WaveAnimation());
    }

    private void OnDisable()
    {
        if(WaveAnim != null)
        {
            StopCoroutine(WaveAnim);
            WaveAnim = null;
        }
    }

    public void OnUIChangeConfirmed()
    {
        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            if (spellslotUI == null)
            {
                continue;
            }
            SpellContainer SC = attackHandler.SpellArray[i];
            SC.spell = spellslotUI.spell;
            if (SC.spell != null)
            {
                SC.TempProjInfo = SC.spell.OGProjectileInformation;
            }
            else
            {
                SC.TempProjInfo = new ProjectileInformation();
            }
            attackHandler.SpellArray[i] = SC;
            spellslotUI.SpellsAffectingIt.Clear();
        }
        attackHandler.DontCast.Clear();
        attackHandler.Turn = 0;

        RefreshAffected();
        InvokeWaveAnim();
    }

    void RefreshAffected()
    {
        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            if (attackHandler.SpellArray[i].spell == null)
            {
                continue;
            }

            List<int> affected = attackHandler.SpellArray[i].spell.FindAffected(i, attackHandler);
            if (affected == null) continue;
            for (int af = 0; af < affected.Count; af++)
            {
                SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(affected[af]).gameObject, out SpellSlotUI spellslotUI);
                if (spellslotUI == null) continue;
                spellslotUI.SpellsAffectingIt.Add(attackHandler.SpellArray[i].spell);
            }
        }
    }
    void RefreshUISpellDeck()
    {
        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            if (spellslotUI == null)
            {
                continue;
            }
            spellslotUI.SetSpell(attackHandler.SpellArray[i].spell, i);
        }
    }
    void OnSpellDeckChangeSize()
    {
        if (transform.childCount == attackHandler.SpellArray.Count)
        {
            return;
        }

        for (int i = 0; i < attackHandler.SpellArray.Count; i++)
        {
            GameObject SpellSlotUIOBJ = ObjectPool.GetObj(SpellSlotUIPrefab.name);
            SpellSlotUIOBJ.transform.SetParent(transform, false);

            SpellSlotUI.TryGetSpellSlotUI(transform.GetChild(i).gameObject, out SpellSlotUI spellslotUI);
            spellslotUI.ParentDeck = this;
        }


        RefreshUISpellDeck();
    }

    IEnumerator WaveAnimation()
    {
        int count = transform.childCount;
        if (count == 0)
        {
            WaveAnim = null;
            yield break;
        }
        float totalCycle = delayBetweenSlots * count + activeDuration;
        float elapsed = 0f;
        while (elapsed < totalCycle)
        {
            float t = elapsed * speed;
            for (int i = 0; i < count; i++)
            {
                float slotStart = i * delayBetweenSlots;
                float localT = t - slotStart;

                float zRotation = 0f;
                if (localT >= 0f && localT <= activeDuration)
                {
                    float p = localT / activeDuration;
                    float ease = Mathf.Sin(p * Mathf.PI);
                    zRotation = ease * amplitude;
                }
                transform.GetChild(i).localRotation = Quaternion.Euler(0f, 0f, zRotation);
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).localRotation = Quaternion.identity;
        WaveAnim = null;
    }


    void InvokeWaveAnim()
    {
        if (WaveAnim == null)
        {
            WaveAnim = StartCoroutine(WaveAnimation());
        }
    }
}
