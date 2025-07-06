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
            fistProj.transform.position = position; // for some reason spawns at the bottom sometimes????

            //fistProj.transform.SetParent(attackHandler.AttackStartPoint.parent);
            //fistProj.transform.localPosition = attackHandler.AttackStartPoint.localPosition;

            fistProj.transform.rotation = rotation;
            fistProj.Owner = attackHandler.Owner;
        }
        Debug.Log("FIST POS: " + fist.transform.position);
        Debug.Log("TARGET POS: " + attackHandler.AttackStartPoint.position);
        Debug.Log("PARENT POS: " + attackHandler.AttackStartPoint.parent.position);
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
