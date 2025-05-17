using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    public List<SpellContainer> SpellArray; // PROBLEM: WHAT HAPPENS WHEN ITS THE SAME MODIFIER SPELL MULTIPLE TIMES?
    public float CastTime = 0.25f; // might turn this into a animation speed multiplier for the punches instead
    public int Turn;
    public Transform AttackStartPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetSpells(0, SpellArray.Count);
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Cast()
    {

        for (int i = Turn; i < SpellArray.Count; i++)
        {
            Turn++;
            SpellArray[i].spell.Apply(i, this, out bool UseTurn);
            if (UseTurn)
            {
                if (Turn >= SpellArray.Count)
                {
                    Turn = 0;
                }
                ResetSpells(0, Turn); // may need to change later if we gonna back spell wrapping (rune at end going to start cuz multicast)
                break;
            }
        }
    }

    private void ResetSpells(int Start, int End)
    {
        for (int i = Start; i < End; i++)
        {
            SpellContainer container = SpellArray[i];
            container.TempProjInfo = container.spell.OGProjectileInformation;
            SpellArray[i] = container;
        }
    }
}
