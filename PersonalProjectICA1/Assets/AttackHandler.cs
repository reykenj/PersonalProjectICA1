using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    public List<SpellContainer> SpellArray; // PROBLEM: WHAT HAPPENS WHEN ITS THE SAME MODIFIER SPELL MULTIPLE TIMES?
    public HashSet<int> DontCast;
    public float CastTime = 0.25f; // might turn this into a animation speed multiplier for the punches instead
    public int Turn;
    public Transform AttackStartPoint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontCast = new HashSet<int>();
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

            if (DontCast.Contains(i)) {
                DontCast.Remove(i);
                Debug.Log("Removed index: " + i);
                return;
            }
            Debug.Log("trying to cast " + SpellArray[i].spell.name);
            SpellArray[i].spell.Apply(i, this, out bool UseTurn, AttackStartPoint.position, AttackStartPoint.rotation);
            if (UseTurn)
            {
                if (Turn >= SpellArray.Count || DontCast.Contains(Turn) && (Turn + 1 >= SpellArray.Count))
                {
                    Turn = 0;
                }
                
                ResetSpells(0, Turn); // may need to change later if we gonna back spell wrapping (rune at end going to start cuz multicast)
                break;
            }
        }
    }

    public void BasicCast(int Index, Vector3 position, Quaternion rotation)
    {
        SpellArray[Index].spell.Apply(Index, this, out bool UseTurn, position, rotation);
        ResetSpells(Index, Index+1);
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

    public int FindNextTurnSpellIndexWrappedOnce(int currentIndex)
    {
        int count = SpellArray.Count;
        if (count == 0)
            return -1;
        int nextIndex = (currentIndex + 1) % count;

        while (nextIndex != currentIndex)
        {
            if (SpellArray[nextIndex].spell.UseTurn)
            {
                return nextIndex;
            }
            nextIndex = (nextIndex + 1) % count;
        }

        return -1;
    }


}
