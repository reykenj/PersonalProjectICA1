using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlerHotbar : MonoBehaviour
{
    [SerializeField] private SpellSlotUI[] slots = new SpellSlotUI[4];

    [SerializeField] private AttackHandler attackHandler;

    [Header("Slot animation positions")]
    [SerializeField] private Vector3[] slotPositions = new Vector3[4];
    [SerializeField] private Vector3[] slotScales = new Vector3[4];

    [SerializeField] private int currentTurn;

    Coroutine UpdateSlots;

    private void OnEnable()
    {
        currentTurn = attackHandler.Turn;
        // Snap to positions initially
        for (int i = 0; i < 4; i++)
        {
            slots[i].transform.localPosition = slotPositions[i];
            slots[i].transform.localScale = slotScales[i];
        }

        for (int i = 0; i < 4; i++)
        {
            int Index = (currentTurn + (i - 2)) % attackHandler.SpellArray.Count;
            if (Index < 0) {
                Index = attackHandler.SpellArray.Count + Index;
            }
            slots[i].SetSpell(attackHandler.SpellArray[Index].spell, 0);
        }

        //slots[3].SetSpell(attackHandler.SpellArray[(currentTurn + 1) % attackHandler.SpellArray.Count].spell, 0);
        //slots[2].SetSpell(attackHandler.SpellArray[(currentTurn) % attackHandler.SpellArray.Count].spell, 0);
        //slots[1].SetSpell(attackHandler.SpellArray[(currentTurn - 1) % attackHandler.SpellArray.Count].spell, 0);
        //slots[0].SetSpell(attackHandler.SpellArray[(currentTurn -2) % attackHandler.SpellArray.Count].spell, 0);
        UpdateSlots = StartCoroutine(UpdateSlotsCoroutine());
    }

    private void OnDisable()
    {
        if(UpdateSlots != null)
        {
            StopCoroutine(UpdateSlots);
            UpdateSlots = null;
        }
    }

    private void Start()
    {
        attackHandler.Casted += OnCast;
    }

    void OnCast()
    {
        if (currentTurn == attackHandler.Turn)
        {
            currentTurn++;
        }
    }
    private IEnumerator UpdateSlotsCoroutine()
    {
        while (true)
        {
            Debug.Log("[HotBar] Updating slots!");
            if (currentTurn != attackHandler.Turn)
            {
                Debug.Log("[HotBar] Trying to animate from update slots!");
                int NextTurn = (currentTurn + 1) % attackHandler.SpellArray.Count;
                AnimateSlots(currentTurn, NextTurn);
                currentTurn = NextTurn;
            }
            yield return new WaitForSeconds(0.26F);
        }
    }

    private void AnimateSlots(int fromTurn, int toTurn)
    {
        Debug.Log("[HotBar] Animating Slots!");


        for (int i = 0; i < slots.Length; i++)
        {
            int Index = (currentTurn + (i - 2)) % attackHandler.SpellArray.Count;
            if (Index < 0)
            {
                Index = attackHandler.SpellArray.Count + Index;
            }
            slots[i].SetSpell(attackHandler.SpellArray[Index].spell, 0);
        }

        SpellSlotUI removedSlot = slots[0];
        for (int i = 0; i < slots.Length - 1; i++)
        {
            slots[i] = slots[i + 1];
        }
        slots[slots.Length - 1] = removedSlot;
        // Animate their positions/sizes
        for (int i = 0; i < slots.Length; i++)
        {
            StartCoroutine(LerpSlot(slots[i], slotPositions[i], slotScales[i], 0.25f));
        }
    }

    private IEnumerator LerpSlot(SpellSlotUI slot, Vector3 targetPos, Vector3 targetScale, float duration)
    {
        Debug.Log("[HotBar] Lerping slot!");
        Vector3 startPos = slot.transform.localPosition;
        Vector3 startScale = slot.transform.localScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            slot.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            slot.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        slot.transform.localPosition = targetPos;
        slot.transform.localScale = targetScale;
    }
}
