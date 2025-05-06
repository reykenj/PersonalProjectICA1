using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    [SerializeField] List<Spell> SpellArray;
    public float CastTime = 0.25f;
    public float timer;
    public int Turn;

    public bool isMainHandler = false;
    public Transform AttackStartPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetSpells();
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }
    public void Cast()
    {
        if (!isMainHandler) return;

        for (int i = Turn; i < SpellArray.Count; i++)
        {
            Spell spell = SpellArray[i];
            if (spell != null)
            {
                Turn++;
                spell.Apply(SpellArray[Mathf.Clamp(i + 1, 0, SpellArray.Count - 1)], this, out bool UseTurn);
                if (UseTurn)
                {
                    timer += CastTime;
                    if (Turn >= SpellArray.Count)
                    {
                        Turn = 0;
                    }
                    ResetSpells(Turn, SpellArray.Count); // may need to change later if we gonna back spell wrapping (rune at end going to start cuz multicast)
                    break;
                }
            }
        }
    }

    private void ResetSpells(int Start, int End)
    {
        for (int i = Start; i < End; i++)
        {
            Spell spell = SpellArray[i];
            spell.SpellReset();
        }
    }
}
