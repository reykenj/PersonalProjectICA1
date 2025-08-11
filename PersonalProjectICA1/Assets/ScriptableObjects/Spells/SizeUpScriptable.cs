using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Size Up Spell", menuName = "Spells/Size Up Spell")]
public class SizeUpSpell : Spell
{
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private float scaleclamp = 10.0f;
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        int IndexToActivateOn = attackHandler.FindNextTurnSpellIndexWrappedOnce(Index);

        if (IndexToActivateOn == -1)
        {
            Debug.Log("Size Up: No valid target spell found.");
            return Index;
        }

        SpellContainer EditedProj = attackHandler.SpellArray[IndexToActivateOn];

 
        if (scaleMultiplier > 0)
        {
            EditedProj.TempProjInfo.ScaleCurveX = MultiplyCurve(EditedProj.TempProjInfo.ScaleCurveX, scaleMultiplier, scaleclamp);
            EditedProj.TempProjInfo.ScaleCurveY = MultiplyCurve(EditedProj.TempProjInfo.ScaleCurveY, scaleMultiplier, scaleclamp);
            EditedProj.TempProjInfo.ScaleCurveZ = MultiplyCurve(EditedProj.TempProjInfo.ScaleCurveZ, scaleMultiplier, scaleclamp);
            EditedProj.TempProjInfo.DestructionRadius = Mathf.Clamp(EditedProj.TempProjInfo.DestructionRadius * scaleMultiplier, 0, scaleclamp);
        }

        attackHandler.SpellArray[IndexToActivateOn] = EditedProj;
        return Index;
    }

    private AnimationCurve MultiplyCurve(AnimationCurve original, float multiplier, float clamp)
    {
        if (original == null || original.length == 0)
            return original;

        Keyframe[] keys = original.keys;
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].value > 0)
            {
                keys[i].value *= multiplier;
            }
            if(keys[i].value > clamp)
            {
                return original;
            }
        }
        return new AnimationCurve(keys);
    }

    public override void OnHit(Projectile projectile)
    {
    }
    public override List<int> FindAffected(int Index, AttackHandler attackHandler)
    {
        return attackHandler.FindAffectedModifier(Index);
    }
}
