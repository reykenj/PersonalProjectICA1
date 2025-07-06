using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Energy Ball Spell", menuName = "Spells/Energy Ball Spell")]
public class EnergyBallSpell : Spell
{
    [SerializeField] GameObject AttackPrefab;
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        GameObject fist = ObjectPool.GetObj("AttackPrefab");
        if (Projectile.TryGetProj(fist, out Projectile fistProj))
        {
            fistProj.SetProjInfo(attackHandler.SpellArray[Index].TempProjInfo);
            fistProj.transform.position = position;
            fistProj.transform.rotation = rotation;
            fistProj.Owner = attackHandler.Owner;
        }
        return Index;
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
