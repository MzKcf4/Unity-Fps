using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssetManager : MonoBehaviour
{
    public static WeaponAssetManager Instance;
    public WeaponNameToResourceConfig weaponNameToResourceConfig;
    public CharacterCommonResources characterCommonResources;
    public WeaponCommonResources weaponCommonResources;
    public GameObject weaponMuzzleFeedbackPrefab;

    private List<string> activeWeaponIdList = new List<string>();
    
    void Awake()
    {
        Instance = this;
    }
        
    public WeaponResources GetWeaponResouce(string weaponName)
    {
        return weaponNameToResourceConfig.GetWeaponResource(weaponName);
    }

    public CharacterCommonResources GetCharacterCommonResources()
    {
        return characterCommonResources;
    }

    public WeaponCommonResources GetWeaponCommonResources()
    {
        return weaponCommonResources;
    }

    public string GetRandomActiveWeaponId()
    {
        if (activeWeaponIdList.Count == 0)
        {
            E_weapon_info.ForEachEntity(weaponInfo => {
                if (weaponInfo.f_active)
                {
                    activeWeaponIdList.Add(weaponInfo.f_name);
                }
            });
        }

        return Utils.GetRandomElement<string>(activeWeaponIdList);
    }
}
