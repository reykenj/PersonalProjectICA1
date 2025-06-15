using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MulticastSpell Spell", menuName = "Spells/MulticastSpell Spell")]
public class MulticastSpell : Spell
{
    [SerializeField] int MaxMulticastCount = 3;
    //[SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
    }


    public override void PreApply(int Index, AttackHandler attackHandler, Vector3 position, Quaternion rotation)
    {
        SpellContainer spellContainer = attackHandler.SpellArray[Index];
        spellContainer.NumList.Clear();
        int MulticastCount = 0;
        int spellCount = attackHandler.SpellArray.Count;
        int CastingIndex = (Index + 1) % spellCount;
        while (true)
        {
            spellContainer.NumList.Add(CastingIndex);
            if (attackHandler.SpellArray[CastingIndex].spell.UseTurn)
            {
                MulticastCount++;
            }
            if (MulticastCount >= MaxMulticastCount)
                break;
            if (CastingIndex == Index)
                break;
            CastingIndex = (CastingIndex + 1) % spellCount;
        }
    }
    public override void OnHit(Projectile projectile)
    {
    }
}
