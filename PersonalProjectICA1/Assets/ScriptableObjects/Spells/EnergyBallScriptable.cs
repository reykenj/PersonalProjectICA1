using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Energy Ball Spell", menuName = "Spells/Energy Ball Spell")]
public class EnergyBallSpell : Spell
{
    [SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn)
    {
        UseTurn = true;
        GameObject fist = ObjectPool.GetObj("AttackPrefab");
        if (Projectile.TryGetProj(fist, out Projectile fistProj))
        {
            fistProj.SetProjInfo(attackHandler.SpellArray[Index].TempProjInfo);
            fistProj.transform.position = attackHandler.AttackStartPoint.position;
            fistProj.transform.rotation = attackHandler.AttackStartPoint.rotation;
            fistProj.Owner = attackHandler.gameObject;
        }
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
