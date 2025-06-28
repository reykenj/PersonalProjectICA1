using NUnit.Framework.Internal.Execution;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "On Collision Spell", menuName = "Spells/On Collision Spell")]
public class OnCollisionSpell : Spell
{
    //[SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;

        int IndexToActivateOn = attackHandler.FindNextTurnSpellIndexWrappedOnce(Index);
        if (IndexToActivateOn == -1)
        {
            return;
        }
        int IndexToCast = attackHandler.FindNextTurnSpellIndexWrappedOnce(IndexToActivateOn);
        if(IndexToCast == -1)
        {
            return;
        }
        Debug.Log("Index to activate on: " + IndexToActivateOn);
        Debug.Log("Index to cast: " + IndexToCast);

        SpellContainer SCActivation = attackHandler.SpellArray[IndexToActivateOn];

        int spellCount = attackHandler.SpellArray.Count;
        int i = (IndexToActivateOn + 1) % spellCount;
        while (i != IndexToCast)
        {
            //Debug.Log("Casting in between: " + attackHandler.SpellArray[i].spell.name);

            if (attackHandler.SpellArray[i].spell != null)
            {
                attackHandler.BasicCast(i, attackHandler.AttackStartPoint.position, attackHandler.AttackStartPoint.rotation);
                attackHandler.DontCast.Add(i);
            }
            i = (i + 1) % spellCount;
            if (i == IndexToActivateOn) break;
        }


        SCActivation.TempProjInfo.OnCollision += (projectile) =>
        {
            Debug.Log("Triggering multiple casts");
            attackHandler.BasicCast(IndexToCast, projectile.transform.position, projectile.transform.rotation);
        };

        attackHandler.SpellArray[IndexToActivateOn] = SCActivation;
        attackHandler.DontCast.Add(IndexToCast);
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
