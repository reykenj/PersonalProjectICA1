using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Punch Spell", menuName = "Spells/Punch Spell")]
public class PunchSpell : Spell
{
    [SerializeField] GameObject AttackPrefab;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn)
    {
        UseTurn = true;
        GameObject fist = ObjectPool.GetObj("AttackPrefab");
        if(Projectile.TryGetProj(fist, out Projectile fistProj))
        {
            fistProj.SetProjInfo(attackHandler.SpellArray[Index].TempProjInfo);
            fistProj.transform.position = attackHandler.AttackStartPoint.position;
            fistProj.transform.rotation = attackHandler.AttackStartPoint.rotation;
            fistProj.Owner = attackHandler.gameObject;
        }
        //int EditIndex = Index + 1;
        //if (EditIndex >= attackHandler.SpellArray.Count) return;
        //SpellContainer EditedProj = attackHandler.SpellArray[EditIndex];
        //EditedProj.TempProjInfo.Render = true;
        //attackHandler.SpellArray[EditIndex] = EditedProj;
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
