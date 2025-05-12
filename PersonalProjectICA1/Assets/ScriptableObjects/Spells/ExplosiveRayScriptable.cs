using NUnit.Framework.Internal.Execution;
using UnityEngine;

[CreateAssetMenu(fileName = "Explosive Ray Spell", menuName = "Spells/Explosive Ray Spell")]
public class ExplosiveRaySpelll : Spell
{
    [SerializeField] GameObject AttackPrefab;
    private float ExplosionDamMult = 5.0f;
    private float ExplosionRange = 2.0f;
    private float BeamRange = 50.0f;
    public override void Apply(int Index, AttackHandler attackHandler, out bool UseTurn)
    {
        UseTurn = true;
        GameObject beam = ObjectPool.GetObj("AttackPrefab");

        if (Projectile.TryGetProj(beam, out Projectile beamProj))
        {
            Keyframe start;
            Keyframe end;

            //Keyframe start = new Keyframe(0f, 6f);
            //Keyframe end = new Keyframe(1f, 6f);
            if (Physics.Raycast(attackHandler.AttackStartPoint.position, attackHandler.AttackStartPoint.forward, out RaycastHit hit, BeamRange, LayerMask.GetMask("Voxel")))
            {
                start = new Keyframe(0f, hit.distance);
                end = new Keyframe(1f, hit.distance);

                Vector3 hitPoint = hit.point - hit.normal * 0.01f;
                Vector3 voxelPos = new Vector3(
                    Mathf.Floor(hitPoint.x),
                    Mathf.Floor(hitPoint.y),
                    Mathf.Floor(hitPoint.z));
                ChunkManager.Instance.RemoveVoxelsInArea(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), ExplosionRange);
                Collider[] colliders = Physics.OverlapSphere(voxelPos + new Vector3(0.5f, 0.5f, 0.5f), ExplosionRange);
                foreach (Collider collider in colliders)
                {

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
            temp.TempProjInfo._initialPosition = attackHandler.AttackStartPoint.position;
            attackHandler.SpellArray[Index] = temp;



            beamProj.SetProjInfo(attackHandler.SpellArray[Index].TempProjInfo);
            beamProj.transform.position = attackHandler.AttackStartPoint.position;
            beamProj.transform.rotation = attackHandler.AttackStartPoint.rotation;
            //beamProj.transform.LookAt(hit.point);
            beamProj.Owner = attackHandler.gameObject;
        }
    }

    public override void OnHit(Projectile projectile)
    {
    }



}
