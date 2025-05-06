using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Punch Spell", menuName = "Spells/Punch Spell")]
public class PunchSpell : Spell
{
    [SerializeField] GameObject AttackPrefab;
    public override void SpellReset()
    {
        TempProjectileInformation = OGProjectileInformation;
    }
    public override void Apply(Spell NextSpell, AttackHandler attackHandler, out bool UseTurn)
    {
        UseTurn = true;
        GameObject fist = ObjectPool.GetObj("AttackPrefab");
        if(Projectile.TryGetProj(fist, out Projectile fistProj))
        {
            fistProj.SetProjInfo(TempProjectileInformation);
            fistProj.transform.position = attackHandler.AttackStartPoint.position;
            fistProj.transform.rotation = attackHandler.AttackStartPoint.rotation;
            fistProj.Owner = attackHandler.gameObject;
        }
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
