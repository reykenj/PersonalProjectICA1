using NUnit.Framework.Internal.Execution;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Damage Up Spell", menuName = "Spells/Damage Up Spell")]
public class DamageUpSpell : Spell
{
    //[SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        int IndexToActivateOn = attackHandler.FindNextTurnSpellIndexWrappedOnce(Index);

        SpellContainer EditedProj = attackHandler.SpellArray[IndexToActivateOn];
        EditedProj.TempProjInfo.Damage += OGProjectileInformation.Damage;
        attackHandler.SpellArray[IndexToActivateOn] = EditedProj;
    }
    public override void OnHit(Projectile projectile)
    {
    }
}
