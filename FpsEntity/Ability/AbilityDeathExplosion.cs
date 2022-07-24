using UnityEngine;
using Kit.Physic;
using System.Collections.Generic;

public class AbilityDeathExplosion : Ability
{
    public static readonly string ID = "death_explosion";

    public AbilityDeathExplosion(FpsCharacter owner) : base(owner)
    {
        owner.onKilledEvent.AddListener(OnOwnerKilled);
    }

    public override void Update(float deltaTime)
    {
        // do nothing
    }

    private void OnOwnerKilled(GameObject owner)
    {
        EffectManager.Instance.RpcPlayEffect(Constants.EFFECT_NAME_GREEN_SPHERE_BLAST, owner.transform.position, 3f);
        RaycastHelper raycastHelper = GetAndSetRaycastHelper();
        raycastHelper.CheckPhysic();

        IEnumerable<Collider> hits = raycastHelper.GetOverlapColliders();
        foreach (Collider collider in hits)
        { 
            FpsCharacter hitChar = collider.GetComponent<FpsCharacter>();
            Debug.Log(hitChar);
            if(hitChar == null || hitChar.team == TeamEnum.Monster) continue;

            hitChar.TakeDamage(DamageInfo.AsDamageInfo(20, this.owner));
        }
    }


    private RaycastHelper GetAndSetRaycastHelper() 
    { 
        RaycastHelper raycastHelper = owner.GetComponent<RaycastHelper>();
        raycastHelper.RayType = RaycastHelper.eRayType.SphereOverlap;
        raycastHelper.SetMemorySize(10);
        raycastHelper.m_Radius = 6f;
        raycastHelper.m_LocalPosition = Vector3.zero;
        return raycastHelper;
    }

    public override string GetID()
    {
        return ID;
    }
}

