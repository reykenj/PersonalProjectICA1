using NUnit.Framework.Internal.Execution;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "On Collision Spell", menuName = "Spells/On Collision Spell")]
public class OnCollisionSpell : Spell
{
    //[SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        //SpellContainer container = attackHandler.SpellArray[Index];
        int IndexToActivateOn = attackHandler.FindNextTurnSpellIndexWrappedOnce(Index);

        //container.NumList.Add(IndexToCast);

        int IndexToCast = attackHandler.FindNextTurnSpellIndexWrappedOnce(IndexToActivateOn);

        Debug.Log("Index to activate on: " + IndexToActivateOn);
        Debug.Log("Index to cast: " + IndexToCast);
        SpellContainer SCActivation = attackHandler.SpellArray[IndexToActivateOn];
        SCActivation.TempProjInfo.OnCollision += (projectile) =>
        {
            Debug.Log("Trying to basic cast this");
            attackHandler.BasicCast(IndexToCast, projectile.transform.position, projectile.transform.rotation);
        };

        attackHandler.SpellArray[IndexToActivateOn] = SCActivation;
        attackHandler.DontCast.Add(IndexToCast);
    }
    public override void OnHit(Projectile projectile)
    {
    }
}
