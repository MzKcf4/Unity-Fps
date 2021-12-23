using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class WeaponNameToResourceConfig : SerializedScriptableObject
{
    public Dictionary<string, WeaponResources> dictNameToWeaponResource;
    
    public WeaponResources GetWeaponResource(string weaponName)
    {
        if(!dictNameToWeaponResource.ContainsKey(weaponName))
            Debug.LogError("Key not exist in WeaponResource : " + weaponName);
            
        return dictNameToWeaponResource[weaponName];
    }
}
