using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MulticastSpell Spell", menuName = "Spells/MulticastSpell Spell")]
public class MulticastSpell : Spell
{
    [SerializeField] int MaxMulticastCount = 3;
    //[SerializeField] GameObject AttackPrefab;
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        return Index;
    }


    public override void PreApply(int Index, AttackHandler attackHandler, Vector3 position, Quaternion rotation)
    {
        SpellContainer spellContainer = attackHandler.SpellArray[Index];
        spellContainer.NumList.Clear();
        int MulticastCount = 0;
        int spellCount = attackHandler.SpellArray.Count;
        int CastingIndex = (Index + 1) % spellCount;
        List<int> CurrNumList = new List<int>();    
        while (true)
        {
            if (MulticastCount >= MaxMulticastCount)
                break;
            if (CastingIndex == Index)
                break;
            if (attackHandler.DontCast.Contains(CastingIndex))
            {
                CastingIndex = (CastingIndex + 1) % spellCount;
                continue;
            }

            CurrNumList.Add(CastingIndex);
            if (attackHandler.SpellArray[CastingIndex].spell.UseTurn)
            {
                spellContainer.NumList.AddRange(CurrNumList);
                attackHandler.DontCast.AddRange(spellContainer.NumList);
                CurrNumList.Clear();
                MulticastCount++;
            }
        }
        attackHandler.SpellArray[Index] = spellContainer;

    }
    public override void OnHit(Projectile projectile)
    {
    }
}
