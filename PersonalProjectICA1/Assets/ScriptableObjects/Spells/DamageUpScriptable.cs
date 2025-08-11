using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage Up Spell", menuName = "Spells/Damage Up Spell")]
public class DamageUpSpell : Spell
{
    //[SerializeField] GameObject AttackPrefab;
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        int IndexToActivateOn = attackHandler.FindNextTurnSpellIndexWrappedOnce(Index);
        if (IndexToActivateOn == -1)
        {
            Debug.Log("Damage Up something is missing");
            return Index;
        }
        SpellContainer EditedProj = attackHandler.SpellArray[IndexToActivateOn];
        EditedProj.TempProjInfo.Damage += OGProjectileInformation.Damage;
        attackHandler.SpellArray[IndexToActivateOn] = EditedProj;
        return Index;
    }
    public override void OnHit(Projectile projectile)
    {
    }
    public override List<int> FindAffected(int Index, AttackHandler attackHandler)
    {
        return attackHandler.FindAffectedModifier(Index);
    }
}
