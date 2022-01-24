using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StreamingAssetManager : MonoBehaviour
{
    public static StreamingAssetManager Instance;
    public Dictionary<string, WeaponResources> dictNameToWeaponResource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildWeaponResourceDict();
    }

    public WeaponResources GetWeaponResouce(string weaponId) 
    {
        return dictNameToWeaponResource[weaponId];
    }

    private void BuildWeaponResourceDict()
    {
        dictNameToWeaponResource = new Dictionary<string, WeaponResources>();
        Addressables.LoadAssetsAsync<WeaponResources>(Constants.LABEL_WEAPON_RESOURCE, (loadedWeaponRes) =>
        {
            if (loadedWeaponRes == null) return;
            dictNameToWeaponResource.Add(loadedWeaponRes.weaponId, loadedWeaponRes);
        });
    }
}
