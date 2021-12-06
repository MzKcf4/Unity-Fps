using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssetManager : MonoBehaviour
{
    public static WeaponAssetManager Instance;
    public WeaponNameToResourceConfig weaponNameToResourceConfig;
    
    void Awake()
    {
        Instance = this;
    }
        
    public WeaponResources GetWeaponResouce(string weaponName)
    {
        return weaponNameToResourceConfig.GetWeaponResource(weaponName);
    }
}
