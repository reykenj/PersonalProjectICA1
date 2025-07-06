using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Tackle Spell", menuName = "Spells/Tackle Spell")]
public class TackleSpell : Spell
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
            fistProj.transform.SetParent(attackHandler.Owner.transform);
            fistProj.Owner = attackHandler.Owner;
        }


        if (Humanoid.TryGetHumanoid(attackHandler.Owner, out Humanoid humanoid))
        {
            humanoid.ExternalVel = fistProj.transform.forward * 25.0f;
        }

        humanoid.OnGrounded += () =>
        {
            ProjectileInformation PI = attackHandler.SpellArray[Index].TempProjInfo;
            PI.lifetime = 0;
            fistProj.SetProjInfo(PI);
        };
        //int EditIndex = Index + 1;
        //if (EditIndex >= attackHandler.SpellArray.Count) return;
        //SpellContainer EditedProj = attackHandler.SpellArray[EditIndex];
        //EditedProj.TempProjInfo.Render = true;
        //attackHandler.SpellArray[EditIndex] = EditedProj;

        return Index;
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
