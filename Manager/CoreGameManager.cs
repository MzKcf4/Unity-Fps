using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
        float spread = fpsWeapon.spread;
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
    
    [ClientRpc]
    public void RpcSpawnFx(List<HitPointInfoDto> hitWallRayList , List<HitPointInfoDto> hitCharacterRayList)
    {
        foreach(HitPointInfoDto hitInfo in hitWallRayList)
            LocalSpawnManager.Instance.SpawnBulletDecalFx(hitInfo.hitPoint , hitInfo.hitPointNormal);
            
        foreach(HitPointInfoDto hitInfo in hitCharacterRayList)
            LocalSpawnManager.Instance.SpawnBloodFx(hitInfo.hitPoint);
    }
    
}
