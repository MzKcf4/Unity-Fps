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
}
