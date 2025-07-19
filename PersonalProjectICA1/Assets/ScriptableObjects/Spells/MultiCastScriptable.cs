using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MulticastSpell Spell", menuName = "Spells/MulticastSpell Spell")]
public class MulticastSpell : Spell
{
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        attackHandler.TempMultiCastCount += MulticastAdditive;
        attackHandler.DontCast.Add(Index);
        return Index;
    }
    public override void PreApply(int Index, AttackHandler attackHandler, Vector3 position, Quaternion rotation)
    {
        attackHandler.TempMultiCastCount += MulticastAdditive;
    }
    public override void OnHit(Projectile projectile)
    {
    }
}
