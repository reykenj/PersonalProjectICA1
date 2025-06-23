using System.Collections.Generic;
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
        int spellCount = SpellArray.Count;
        if (spellCount == 0) return;

        int startTurn = Turn;
        //bool hasWrapped = false;

        while (true)
        {
            if (Turn >= spellCount)
                Turn = 0;

            if (DontCast.Contains(Turn))
            {
                DontCast.Remove(Turn);
                Debug.Log("Removed index: " + Turn);
                Turn++;
                continue;
            }
            if (SpellArray[Turn].spell == null)
            {
                Turn++;
                continue;
            }

            Debug.Log("Trying to cast: " + SpellArray[Turn].spell.name);
            SpellArray[Turn].spell.Apply(Turn, this, out bool UseTurn, AttackStartPoint.position, AttackStartPoint.rotation);
            Turn++;

            if (UseTurn)
            {
                if (Turn >= spellCount) Turn = 0;
                ResetSpells(0, Turn);
                break;
            }

            if (Turn == startTurn)
            {
                ResetSpells(0, spellCount);
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
            if(SpellArray[i].spell == null)
            {
                continue;
            }
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
            if (SpellArray[nextIndex].spell == null)
            {
                continue;
            }
            if (SpellArray[nextIndex].spell.UseTurn)
            {
                return nextIndex;
            }
            nextIndex = (nextIndex + 1) % count;
        }

        return -1;
    }


}
