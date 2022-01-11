using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using BansheeGz.BGDatabase;
using Kit.Physic;

public class CoreGameManager : NetworkBehaviour
{
    public static CoreGameManager Instance;
    private static LayerMask MASK_HITBOX;
    private static LayerMask MASK_WALL;
    private static LayerMask MASK_HITBOX_AND_WALL;
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        MASK_HITBOX = LayerMask.GetMask(Constants.LAYER_HITBOX);
        MASK_WALL = LayerMask.GetMask(Constants.LAYER_GROUND);
        MASK_HITBOX_AND_WALL = (1 << MASK_HITBOX.value) | (1 << MASK_WALL.value);
    }
    
    [Server]
    public void DoWeaponRaycast(FpsCharacter character, FpsWeapon fpsWeapon, Vector3 fromPos, Vector3 direction)
    {
        float spread = processSpread(fpsWeapon , character);
        int mask = (LayerMask.GetMask(Constants.LAYER_HITBOX , Constants.LAYER_GROUND, Constants.LAYER_LOCAL_PLAYER_HITBOX));
        
        // Gather the hit points from valid hits on walls / entities
        List<HitPointInfoDto> hitWallRayList = new List<HitPointInfoDto>();
        List<HitPointInfoDto> hitCharacterRayList = new List<HitPointInfoDto>();
        
        for(int i = 0 ; i < fpsWeapon.palletPerShot ; i++)
        {
            RayHitInfo hitInfo = Utils.CastRayAndGetHitInfo(character, fromPos , direction , mask , spread);
            
            if(hitInfo == null)
                continue;
                
            GameObject objOnHit = hitInfo.hitObject;
            // Hits wall
            if( (1 << objOnHit.layer) == MASK_WALL.value)
            {
                hitWallRayList.Add(hitInfo.asHitPointInfoDto());
                continue;
            }
            
            // Else should expect hitting hitbox
            FpsHitbox enemyHitBox = objOnHit.GetComponent<FpsHitbox>();
            FpsEntity hitEntity = enemyHitBox.fpsEntity;
                
            if(hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if(hitTeam == character.team)   
                    continue;
            }
                
            if(hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, enemyHitBox , hitInfo.hitPoint);
                if(character is FpsPlayer)
                {
                    ((FpsPlayer)character).TargetOnWeaponHitEnemy(character.netIdentity.connectionToClient);    
                }
                
                hitEntity.TakeDamage(dmgInfo);
                hitCharacterRayList.Add(hitInfo.asHitPointInfoDto());
                continue;
            }
        }
        
        RpcSpawnFx(hitWallRayList , hitCharacterRayList);
    }
    
    public HitInfoDto DoLocalWeaponRaycast(FpsCharacter character, FpsWeapon fpsWeapon, Vector3 fromPos, Vector3 direction)
    {
        if (fpsWeapon.weaponCategory == WeaponCategory.Melee)
            return DoMeleeWeaponRaycast(character, fpsWeapon);

        // Non-melee handling
        float spread = processSpread(fpsWeapon , character);
        int mask = (LayerMask.GetMask(Constants.LAYER_HITBOX , Constants.LAYER_GROUND, Constants.LAYER_LOCAL_PLAYER_HITBOX));
        
        // Gather the hit points from valid hits on walls / entities
        List<HitWallInfoDto> hitWallDtoList = new List<HitWallInfoDto>();
        List<HitEntityInfoDto> hitEntityDtoList = new List<HitEntityInfoDto>();
        
        HitInfoDto hitInfoDto = new HitInfoDto(hitEntityDtoList , hitWallDtoList);
        
        for(int i = 0 ; i < fpsWeapon.palletPerShot ; i++)
        {
            RayHitInfo hitInfo = Utils.CastRayAndGetHitInfo(character, fromPos , direction , mask , spread);
            
            if(hitInfo == null)
                continue;
            GameObject objOnHit = hitInfo.hitObject;
            // Hits wall
            if( (1 << objOnHit.layer) == MASK_WALL.value)
            {
                hitWallDtoList.Add(hitInfo.asHitWallInfoDto());
                continue;
            }
            
            // Else should expect hitting hitbox
            FpsHitbox enemyHitBox = objOnHit.GetComponent<FpsHitbox>();
            FpsEntity hitEntity = enemyHitBox.fpsEntity;
                
            if(hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if(hitTeam == character.team)   
                    continue;
            }
                
            if(hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, enemyHitBox , hitInfo.hitPoint);                
                HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                {
                    attackerIdentity = character.netIdentity,
                    victimIdentity = hitEntity.netIdentity,
                    damageInfo = dmgInfo
                };
                
                hitEntityDtoList.Add(hitEntityInfoDto);
                continue;
            }
        }
        
        // RpcSpawnFx(hitWallRayList , hitCharacterRayList);
        return hitInfoDto;
    }

    public HitInfoDto DoMeleeWeaponRaycast(FpsCharacter fpsCharacter, FpsWeapon fpsWeapon)
    {
        RaycastHelper meleeRaycastHelper = fpsCharacter.meleeRaycastHelper;
        if (meleeRaycastHelper == null)
            return null;

        meleeRaycastHelper.CheckPhysic();
        IEnumerable<Collider> hits = meleeRaycastHelper.GetOverlapColliders();
        if (hits == null) 
            return null;

        float shortestDist = float.MaxValue;
        GameObject cloestObject = null;

        foreach (Collider hit in hits)
        {
            float newDistance = Vector3.Distance(hit.gameObject.transform.position, fpsCharacter.transform.position);
            if (newDistance < shortestDist)
            {
                shortestDist = newDistance;
                cloestObject = hit.gameObject;
            }
        }
        if (cloestObject == null)
            return null;

        // Gather the hit points from valid hits on walls / entities
        List<HitWallInfoDto> hitWallDtoList = new List<HitWallInfoDto>();
        List<HitEntityInfoDto> hitEntityDtoList = new List<HitEntityInfoDto>();

        HitInfoDto hitInfoDto = new HitInfoDto(hitEntityDtoList, hitWallDtoList);

        // Hits wall
        if ((1 << cloestObject.layer) == MASK_WALL.value)
        {
            hitWallDtoList.Add(new HitWallInfoDto());
        }
        else
        {
            // Else should expect hitting hitbox
            FpsHitbox enemyHitBox = cloestObject.GetComponent<FpsHitbox>();
            FpsEntity hitEntity = enemyHitBox.fpsEntity;

            if (hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if (hitTeam == fpsCharacter.team)
                    return null;
            }

            if (hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, enemyHitBox, enemyHitBox.transform.position);
                HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                {
                    attackerIdentity = fpsCharacter.netIdentity,
                    victimIdentity = hitEntity.netIdentity,
                    damageInfo = dmgInfo
                };

                hitEntityDtoList.Add(hitEntityInfoDto);
            }

        }
        return hitInfoDto;
    }
    
    private float processSpread(FpsWeapon weapon , FpsCharacter character)
    {
        float baseSpread = weapon.spread;
        float movementSpread = weapon.spreadInMove * (character.GetMovementVelocity().magnitude / 5.5f);
        return baseSpread + movementSpread;
        
    }
    
    
    [ClientRpc]
    public void RpcSpawnFx(List<HitPointInfoDto> hitWallRayList , List<HitPointInfoDto> hitCharacterRayList)
    {
        foreach(HitPointInfoDto hitInfo in hitWallRayList)
            LocalSpawnManager.Instance.SpawnBulletDecalFx(hitInfo.hitPoint , hitInfo.hitPointNormal);
            
        foreach(HitPointInfoDto hitInfo in hitCharacterRayList)
            LocalSpawnManager.Instance.SpawnBloodFx(hitInfo.hitPoint);
    }
    
    [ClientRpc]
    public void RpcSpawnFxHitInfo(HitInfoDto hitInfoDto)
    {
        foreach(HitWallInfoDto hitInfo in hitInfoDto.hitWallInfoDtoList)
            LocalSpawnManager.Instance.SpawnBulletDecalFx(hitInfo.hitPoint , hitInfo.hitPointNormal);
        
        foreach(HitEntityInfoDto hitInfo in hitInfoDto.hitEntityInfoDtoList)
            LocalSpawnManager.Instance.SpawnBloodFx(hitInfo.damageInfo.hitPoint);
    }
}
