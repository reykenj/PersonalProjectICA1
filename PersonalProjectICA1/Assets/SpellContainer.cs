using System;
using UnityEngine;

[Serializable]
public struct SpellContainer // The purpose of this spell container is to have multiple copies of "spell" that have different modified values to exist at the same time
{
    public Spell spell;
    public ProjectileInformation TempProjInfo;
}
