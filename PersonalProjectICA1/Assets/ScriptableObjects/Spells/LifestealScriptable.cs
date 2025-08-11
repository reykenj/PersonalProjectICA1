using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Lifesteal Spell", menuName = "Spells/Lifesteal Spell")]
public class LifestealSpell : Spell
{
    [SerializeField] private float lifestealMultiplier = 0.25f;
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


        EditedProj.TempProjInfo.OnCollisionEnemy += (projectile) =>
        {
            Humanoid.TryGetHumanoid(attackHandler.Owner, out Humanoid thishumanoid);
            thishumanoid.Hurt(-EditedProj.TempProjInfo.Damage * lifestealMultiplier);
        };

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
            if (keys[i].value > clamp)
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
