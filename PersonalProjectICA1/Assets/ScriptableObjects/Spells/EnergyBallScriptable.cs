using NUnit.Framework.Internal.Execution;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Energy Ball Spell", menuName = "Spells/Energy Ball Spell")]
public class EnergyBallSpell : Spell
{
    [SerializeField] GameObject AttackPrefab;
    public List<AudioResource> FireSFXs;
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

            if (FireSFXs.Count > 0)
            {
                GameObject SFXOB = ObjectPool.GetObj("SFXGO");
                ReturnAudio.TryGetAudio(SFXOB, out ReturnAudio audio);
                audio.SetAudio(FireSFXs[Random.Range(0, FireSFXs.Count)]);
                audio.transform.position = position;
            }
            //attackHandler.AttackStartPointRM.ApplyRecoil(new Vector3(-2, 2, 0.35f));
        }
        return Index;
    }

    public override void OnHit(Projectile projectile)
    {
    }
}
