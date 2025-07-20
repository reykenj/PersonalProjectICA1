using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class AttackHandler : MonoBehaviour
{
    public List<SpellContainer> SpellArray; // PROBLEM: WHAT HAPPENS WHEN ITS THE SAME MODIFIER SPELL MULTIPLE TIMES?
    public HashSet<int> DontCast;
    public float CastTime = 0.25f; // might turn this into a animation speed multiplier for the punches instead
    public int Turn;
    public Transform AttackStartPoint;
    //public RecoilManager AttackStartPointRM;
    public GameObject Owner;
    public int NaturalMultiCastCount = 1;
    public int TempMultiCastCount = 0;
    public System.Action Casted;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Owner == null)
        {
            Owner = gameObject;
        }
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

        while (true)
        {
            if (DontCast.Contains(Turn))
            {
                DontCast.Remove(Turn);
                Debug.Log("Removed index: " + Turn);
            }
            else
            {
                bool UseTurn = false;

                if (SpellArray[Turn].spell != null)
                {
                    Debug.Log("Trying to cast: " + SpellArray[Turn].spell.name);
                    SpellArray[Turn].spell.Apply(Turn, this, out UseTurn, AttackStartPoint.position, AttackStartPoint.rotation);
                }

                if (UseTurn)
                {
                    Turn = (Turn + 1) % spellCount;
                    ResetSpells(0, Turn);
                    break;
                }
            }

            Turn = (Turn + 1) % spellCount;

            if (Turn == startTurn)
            {
                ResetSpells(0, spellCount);
                break;
            }
        }
    }



    public void MultiCast(Vector3 position, Quaternion rotation, int InsertedTurn = -1, int TempTempMultiCastCount = -1)
    {

        bool FakeTurn = true;
        bool NoMultiCastInserted = true;
        if (InsertedTurn == -1)
        {
            DontCast.Clear();
            FakeTurn = false;
            InsertedTurn = Turn;
        }
        NoMultiCastInserted = TempTempMultiCastCount == -1;
        int spellCount = SpellArray.Count;
        if (spellCount == 0) return;
        int startTurn = InsertedTurn;
        int multicount = 0;
        List<int> Modifier = new List<int>();
        List<int> ApplyModifierAll = new List<int>();
        while (true)
        {
            if (DontCast.Contains(InsertedTurn))
            {
                if (!FakeTurn)
                {
                    DontCast.Remove(InsertedTurn);
                    Debug.Log("Removed index: " + InsertedTurn);
                }
            }
            else
            {
                bool UseTurn = false;

                if (SpellArray[InsertedTurn].spell != null)
                {
                    Debug.Log("Trying to cast: " + SpellArray[InsertedTurn].spell.name);

                    if (SpellArray[InsertedTurn].spell.UseTurn)
                    {
                        for (int i = 0; i < ApplyModifierAll.Count; i++)
                        {
                            SpellArray[ApplyModifierAll[i]].spell.Apply(InsertedTurn - 1, this, out UseTurn, position, rotation);
                        }
                    }
                    InsertedTurn = SpellArray[InsertedTurn].spell.Apply(InsertedTurn, this, out UseTurn, position, rotation);
                }

                if (UseTurn)
                {
                    multicount++;
                    if (NoMultiCastInserted)
                    {
                        TempTempMultiCastCount = TempMultiCastCount;
                    }
                    if (multicount >= NaturalMultiCastCount + TempTempMultiCastCount)
                    {
                        InsertedTurn = (InsertedTurn + 1) % spellCount;
                        ResetSpells(0, InsertedTurn);
                        TempMultiCastCount = 0;
                        break;
                    }
                }
                else if (SpellArray[InsertedTurn].spell != null)
                {
                    Modifier.Add(InsertedTurn);
                    if (SpellArray[InsertedTurn].spell.ApplyToAllModifier)
                    {
                        ApplyModifierAll.Add(InsertedTurn);
                    }
                }
            }

            InsertedTurn = (InsertedTurn + 1) % spellCount;

            if (InsertedTurn == startTurn)
            {
                ResetSpells(0, spellCount);
                break;
            }
        }
        if (!FakeTurn)
        {
            Turn = InsertedTurn;
        }
        DontCast.AddRange(Modifier);
    }
    public int FindMultiCastTurn(int InsertedTurn = -1)
    {
        //Debug.Log("[InsertedLog] " +  InsertedTurn);
        if (InsertedTurn == -1)
        {
            InsertedTurn = Turn;
        }
        int TempTempMultiCastCount = TempMultiCastCount;
        int spellCount = SpellArray.Count;
        if (spellCount == 0) return -1;
        int startTurn = InsertedTurn;
        int multicount = 0;
        List<Spell> ApplyModifierAll = new List<Spell>();
        while (true)
        {
            if (!DontCast.Contains(InsertedTurn))
            {
                if (SpellArray[InsertedTurn].spell != null)
                {
                    if (SpellArray[InsertedTurn].spell.MulticastAdditive > 0)
                    {
                        TempTempMultiCastCount += SpellArray[InsertedTurn].spell.MulticastAdditive;
                    }
                    if (SpellArray[InsertedTurn].spell.UseTurn)
                    {
                        multicount++;
                        if (multicount >= NaturalMultiCastCount + TempTempMultiCastCount)
                        {
                            InsertedTurn = (InsertedTurn + 1) % spellCount;
                            break;
                        }

                    }
                }
            }
            //Debug.Log("[InsertedLog] Added " + InsertedTurn);
            InsertedTurn = (InsertedTurn + 1) % spellCount;
            //Debug.Log("[InsertedLog] Added2 " + InsertedTurn);
            if (InsertedTurn == startTurn)
            {
                break;
            }
        }
        Debug.Log("[InsertedLog] Updated" + InsertedTurn);

        InsertedTurn--;
        if (InsertedTurn < 0)
        {
            InsertedTurn = spellCount + InsertedTurn;
        }
        return InsertedTurn;
    }

    public void BasicCast(int Index, Vector3 position, Quaternion rotation)
    {
        if (SpellArray[Index].spell != null)
        {
            SpellArray[Index].spell.Apply(Index, this, out bool UseTurn, position, rotation);
            ResetSpells(Index, Index + 1);
        }
        else
        {
            Debug.LogError("Tried to basic cast a null spell!");
        }
    }

    public void BasicPreApply(int Index, Vector3 position, Quaternion rotation)
    {
        if (SpellArray[Index].spell != null)
        {
            SpellArray[Index].spell.PreApply(Index, this, position, rotation);
        }
        else
        {
            Debug.LogError("Tried to basic precast a null spell!");
        }
    }
    private void ResetSpells(int Start, int End)
    {
        for (int i = Start; i < End; i++)
        {
            SpellContainer container = SpellArray[i];
            if(container.spell == null)
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
                nextIndex = (nextIndex + 1) % count;
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
