using NUnit.Framework.Internal.Execution;
using UnityEngine;
using static UnityEngine.UI.Image;

[CreateAssetMenu(fileName = "Explosive Ray Spell", menuName = "Spells/Explosive Ray Spell")]
public class ExplosiveRaySpelll : Spell
{
    [SerializeField] GameObject AttackPrefab;
    private float ExplosionDamMult = 5.0f;
    private float ExplosionRange = 2.0f;
    private float BeamRange = 50.0f;
    public override int Apply(int Index, AttackHandler attackHandler, out bool UseTurn, Vector3 position, Quaternion rotation)
    {
        UseTurn = this.UseTurn;
        GameObject beam = ObjectPool.GetObj("AttackPrefab");

        if (Projectile.TryGetProj(beam, out Projectile beamProj))
        {
            Keyframe start;
            Keyframe end;

            //Keyframe start = new Keyframe(0f, 6f);
            //Keyframe end = new Keyframe(1f, 6f);
            if (Physics.Raycast(position, rotation * Vector3.forward, out RaycastHit hit, BeamRange, LayerMask.GetMask("Voxel")))
            {
                start = new Keyframe(0f, hit.distance);
                end = new Keyframe(1f, hit.distance);

                Vector3 hitPoint = hit.point - hit.normal * 0.01f;
                Vector3 voxelPos = new Vector3(
                    Mathf.Floor(hitPoint.x),
                    Mathf.Floor(hitPoint.y),
                    Mathf.Floor(hitPoint.z));
                ChunkManager.Instance.RemoveVoxelsInArea(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), ExplosionRange * FindHighestCurve(attackHandler.SpellArray[Index].TempProjInfo.ScaleCurveX));
                Collider[] colliders = Physics.OverlapSphere(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), ExplosionRange * FindHighestCurve(attackHandler.SpellArray[Index].TempProjInfo.ScaleCurveX));
                foreach (Collider collider in colliders)
                {
                    if(collider.gameObject == attackHandler.Owner) continue;
                    if (Humanoid.TryGetHumanoid(collider.gameObject, out Humanoid hurtcontroller))
                    {
                        hurtcontroller.Hurt(attackHandler.SpellArray[Index].TempProjInfo.Damage * ExplosionDamMult);
                    }
                }
            }
            else
            {
                start = new Keyframe(0f, BeamRange);
                end = new Keyframe(1f, BeamRange);
            }

            start.inTangent = 0;
            start.outTangent = 0;
            end.inTangent = 0;
            end.outTangent = 0;

            AnimationCurve constantCurve = new AnimationCurve(start, end);
            SpellContainer temp = attackHandler.SpellArray[Index];
            temp.TempProjInfo.ScaleCurveZ = constantCurve;
            temp.TempProjInfo._initialPosition = position;
            attackHandler.SpellArray[Index] = temp;



            beamProj.SetProjInfo(attackHandler.SpellArray[Index].TempProjInfo);
            beamProj.transform.position = position;
            beamProj.transform.rotation = rotation;
            //beamProj.transform.LookAt(hit.point);
            beamProj.Owner = attackHandler.Owner;

            //attackHandler.AttackStartPointRM.ApplyRecoil(new Vector3(-2, 2, 0.35f) * 3);
        }

        return Index;
    }

    public override void OnHit(Projectile projectile)
    {
    }

    private float FindHighestCurve(AnimationCurve original)
    {
        if (original == null || original.length == 0)
            return -1;

        Keyframe[] keys = original.keys;

        float highestval = float.MinValue;
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].value > highestval)
            {
                highestval = keys[i].value; ;
            }
        }
        return highestval;
    }




}
