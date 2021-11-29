using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAssetManager : MonoBehaviour
{
    public static WeaponAssetManager Instance;
    public WeaponNameToPrefabConfig weaponNameToPrefabConfig;
    
    // public GameObject ak47ViewPrefab;
    public GameObject ak47WeaponPrefab;
    
    // public GameObject r8ViewPrefab;
    public GameObject r8WeaponPrefab;
    
    // public GameObject sawoffViewPrefab;
    public GameObject sawoffWeaponPrefab;
    
    void Awake()
    {
        Instance = this;
    }
    
    public GameObject GetWeaponPrefab(string weaponName)
    {
        return weaponNameToPrefabConfig.GetPrefab(weaponName);
    }
}
