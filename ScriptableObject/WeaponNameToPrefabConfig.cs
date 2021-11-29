using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu]
public class WeaponNameToPrefabConfig : SerializedScriptableObject
{
    public Dictionary<string, GameObject> dictNameToWeaponPrefab;
    
    public GameObject GetPrefab(string weaponName)
    {
        return dictNameToWeaponPrefab[weaponName];
    }
}
