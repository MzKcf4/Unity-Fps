
using Micosmo.SensorToolkit;
using Mono.CSharp;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Analytics;

public class WeaponRaycastUtils
{

    public static HitInfoDto DoLocalWeaponRaycast(FpsHumanoidCharacter character, FpsWeapon fpsWeapon, Vector3 fromPos, Vector3 direction)
    {
        // Non-melee handling
        float spread = processSpread(fpsWeapon, character);
        int mask = (LayerMask.GetMask(Constants.LAYER_HITBOX, Constants.LAYER_GROUND, Constants.LAYER_LOCAL_PLAYER_HITBOX, Constants.LAYER_CHARACTER_MODEL));

        // Gather the hit points from valid hits on walls / entities
        List<HitWallInfoDto> hitWallDtoList = new List<HitWallInfoDto>();
        List<HitEntityInfoDto> hitEntityDtoList = new List<HitEntityInfoDto>();

        HitInfoDto hitInfoDto = new HitInfoDto(hitEntityDtoList, hitWallDtoList);

        for (int i = 0; i < fpsWeapon.palletPerShot; i++)
        {
            List<RayHitInfo> hitInfos = Utils.CastRayAndGetHitInfo(character, fromPos, direction, mask, spread);
            if (hitInfos.Count == 0)
                continue;

            int wallPenetrated = 0;
            for (int j = 0; j < hitInfos.Count; j++)
            {
                var hitInfo = hitInfos[j];
                if (hitInfo.isHitWall)
                {
                    if (j == 0)
                        hitWallDtoList.Add(hitInfo.asHitWallInfoDto());
                    wallPenetrated++;

                    if (fpsWeapon.penetrationPower - wallPenetrated < 0)
                        break;
                }
                else
                {
                    // Else should expect hitting hitbox
                    GameObject objOnHit = hitInfo.hitObject;
                    FpsHitbox enemyHitBox = objOnHit.GetComponent<FpsHitbox>();
                    if (enemyHitBox == null)
                    {
                        Debug.LogError("Hitbox is null on hit object " + objOnHit.name);
                        return null;
                    }
                    FpsEntity hitEntity = enemyHitBox.fpsEntity;

                    if (hitEntity is FpsCharacter)
                    {
                        TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                        if (hitTeam == character.team)
                            continue;
                    }

                    if (hitEntity != null)
                    {
                        DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, enemyHitBox, hitInfo.hitPoint, true, wallPenetrated);
                        HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                        {
                            attackerIdentity = character.netIdentity,
                            victimIdentity = hitEntity.netIdentity,
                            damageInfo = dmgInfo
                        };

                        hitEntityDtoList.Add(hitEntityInfoDto);
                        // Stop when hit an entity
                        break;
                    }
                }
            }
        }

        return hitInfoDto;
    }


    
    public static HitInfoDto DoLocalMeleeWeaponRaycast(FpsHumanoidCharacter fpsCharacter, FpsWeapon fpsWeapon,Vector3 fromPos, bool isPrimary)
    {
        RaySensor meleeArcSensor = fpsCharacter.meleeArcSensor;
        RaySensor meleeRaySensor = fpsCharacter.meleeRaySensor;
        E_melee_info melee_Info = fpsWeapon.meleeInfo;

        if (meleeArcSensor == null || meleeRaySensor == null || melee_Info == null)
            return null;

        float arcSize = isPrimary ? melee_Info.f_primary_arc : melee_Info.f_secondary_arc;
        meleeRaySensor.Length = isPrimary ? melee_Info.f_primary_range : melee_Info.f_secondary_range;

        meleeArcSensor.SetBoxShape(new Vector3(arcSize, 0.3f, 0.4f));

        // Gather the hit points from valid hits on walls / entities
        List<HitWallInfoDto> hitWallDtoList = new List<HitWallInfoDto>();
        List<HitEntityInfoDto> hitEntityDtoList = new List<HitEntityInfoDto>();
        HitInfoDto hitInfoDto = new HitInfoDto(hitEntityDtoList, hitWallDtoList);

        // Ray check first
        // meleeRaySensor.Clear();
        meleeRaySensor.Pulse();
        List<GameObject> detectedObjects = meleeRaySensor.GetDetectionsByDistance();

        if (detectedObjects.Count > 0)
        { 
            var firshtDetectedObj = detectedObjects[0];
            if (Utils.IsInLayerMask(firshtDetectedObj , LayerMask.GetMask(Constants.LAYER_GROUND)))
            {
                hitWallDtoList.Add(new HitWallInfoDto());
                Debug.Log("Melee hit wall first");
                return hitInfoDto;
            }
            else
            {
                List<FpsHitbox> detectedHitBoxes = new List<FpsHitbox>();
                meleeRaySensor.GetDetectedComponentsByDistance<FpsHitbox>(detectedHitBoxes);

                // If hits 0 hitbix , proceed to do arc check
                if (detectedHitBoxes.Count != 0)
                {
                    FpsHitbox enemyHitBox = detectedHitBoxes[0];
                    FpsEntity hitEntity = enemyHitBox.fpsEntity;
                    if (hitEntity is FpsCharacter)
                    {
                        TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                        if (hitTeam == fpsCharacter.team)
                            return null;
                    }

                    DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, enemyHitBox, enemyHitBox.transform.position, isPrimary, 0);
                    HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                    {
                        attackerIdentity = fpsCharacter.netIdentity,
                        victimIdentity = hitEntity.netIdentity,
                        damageInfo = dmgInfo
                    };

                    hitEntityDtoList.Add(hitEntityInfoDto);
                    Debug.Log("Melee hit a hitbox!");
                    return hitInfoDto;
                }
            }
        }


        // Ray hit nothing , do arc check   
        // meleeArcSensor.Clear();
        meleeArcSensor.Pulse();
        detectedObjects = meleeArcSensor.GetDetectionsByDistance();
        foreach (var obj in detectedObjects)
        {
            Debug.Log("Melee arc detected object: " + obj.name);
        }

        if (detectedObjects.Count > 0)
        {
            foreach (var obj in detectedObjects)
            {
                Debug.Log("Melee arc detected object: " + obj.name);
            }

            List<FpsHitbox> detectedHitBoxes = new List<FpsHitbox>();
            meleeArcSensor.GetDetectedComponentsByDistance<FpsHitbox>(detectedHitBoxes);

            if(detectedHitBoxes.Count == 0)
            {
                // No hitbox detected but has detected objects , should be wall
                var firshtDetectedObj = detectedObjects[0];
                hitWallDtoList.Add(new HitWallInfoDto());
                return hitInfoDto;
            }

            // Got some hitboxes
            HashSet<FpsEntity> hitEntities = new HashSet<FpsEntity>();
            List<FpsHitbox> validHitBoxes = new List<FpsHitbox>();

            foreach (var hitbox in detectedHitBoxes)
            {
                Debug.Log("Melee arc detected hitbox of entity: " + hitbox);
                // Check if this hitbox is blocked by wall
                var hitInfo = Utils.CastRayAndGetHitInfo(fpsCharacter, fromPos, (hitbox.transform.position - fromPos).normalized, LayerMask.GetMask(Constants.LAYER_HITBOX, Constants.LAYER_GROUND), 0f);
                if (hitInfo.Count == 0 || hitInfo[0].isHitWall)
                {
                    Debug.Log("Melee arc hitbox blocked by wall: " + hitbox);
                    continue;
                    // ToDo: return a HitWallInfoDto;
                }

                // Hit hitbox of an entity only once
                FpsEntity hitEntity = hitbox.fpsEntity;
                if (!hitEntities.Contains(hitEntity))
                {
                    hitEntities.Add(hitEntity);
                    validHitBoxes.Add(hitbox);
                }
            }

            if (validHitBoxes.Count == 0)
            {
                // All hitboxes are blocked by wall
                hitWallDtoList.Add(new HitWallInfoDto());
            }

            bool isSingleTarget = isPrimary && fpsWeapon.meleeInfo.f_primary_multi_target
                               || !isPrimary && fpsWeapon.meleeInfo.f_secondary_multi_target;

            if (isSingleTarget)
            {
                var hitbox = validHitBoxes[0];
                FpsEntity hitEntity = hitbox.fpsEntity;
                if (hitEntity is FpsCharacter)
                {
                    TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                    if (hitTeam == fpsCharacter.team)
                        return null;
                }

                DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, hitbox, hitbox.transform.position, isPrimary, 0);
                HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                {
                    attackerIdentity = fpsCharacter.netIdentity,
                    victimIdentity = hitbox.fpsEntity.netIdentity,
                    damageInfo = dmgInfo
                };
                hitEntityDtoList.Add(hitEntityInfoDto);
            }
            else
            {
                foreach (var hitbox in validHitBoxes)
                {
                    FpsEntity hitEntity = hitbox.fpsEntity;
                    if (hitEntity is FpsCharacter)
                    {
                        TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                        if (hitTeam == fpsCharacter.team)
                            continue;
                    }

                    DamageInfo dmgInfo = DamageInfo.AsDamageInfo(fpsWeapon, hitbox, hitbox.transform.position, isPrimary, 0);
                    HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                    {
                        attackerIdentity = fpsCharacter.netIdentity,
                        victimIdentity = hitbox.fpsEntity.netIdentity,
                        damageInfo = dmgInfo
                    };
                    hitEntityDtoList.Add(hitEntityInfoDto);

                }
            }

            return hitInfoDto;
        }
        return null;
    }
    

    private static void processMeleeHitboxes(FpsHumanoidCharacter character, FpsWeapon weapon, List<FpsHitbox> hitboxes, List<HitEntityInfoDto> hitEntityDtoList)
    {
        /*
        foreach (var hitbox in hitboxes)
        {
            FpsEntity hitEntity = hitbox.fpsEntity;
            if (hitEntity is FpsCharacter)
            {
                TeamEnum hitTeam = ((FpsCharacter)hitEntity).team;
                if (hitTeam == character.team)
                    continue;
            }
            if (hitEntity != null)
            {
                DamageInfo dmgInfo = DamageInfo.AsMeleeDamageInfo(weapon, hitbox, true);
                HitEntityInfoDto hitEntityInfoDto = new HitEntityInfoDto()
                {
                    attackerIdentity = character.netIdentity,
                    victimIdentity = hitEntity.netIdentity,
                    damageInfo = dmgInfo
                };
                hitEntityDtoList.Add(hitEntityInfoDto);
            }
        }*/
    }

    private static float processSpread(FpsWeapon weapon, FpsCharacter character)
    {
        float baseSpread = weapon.GetEffectiveSpread();
        float movementSpread = weapon.spreadInMove * (character.GetMovementVelocity().magnitude / 5.5f);
        return baseSpread + movementSpread;

    }
}