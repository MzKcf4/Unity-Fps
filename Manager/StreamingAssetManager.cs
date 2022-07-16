using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using MoreMountains.Feedbacks;

public class StreamingAssetManager : MonoBehaviour
{
    public static StreamingAssetManager Instance;
    public Dictionary<string, GameObject> DictEffectNameToPrefab { get { return dictEffectNameToPrefab; } }

    public Dictionary<string, WeaponResources> dictNameToWeaponResource;
    public Dictionary<string, GameObject> dictMonsterNameToPrefab;
    public Dictionary<string, GameObject> dictEffectNameToPrefab;

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

    public GameObject GetMonsterPrefab(string prefabName)
    {
        return dictMonsterNameToPrefab[prefabName];
    }

    public void InitializeMonsterDict() 
    {
        if (dictMonsterNameToPrefab != null) 
        {
            Debug.LogWarning("Monster dict already initialized!");
            return;
        }

        dictMonsterNameToPrefab = new Dictionary<string, GameObject>();
        Addressables.LoadAssetsAsync<GameObject>(Constants.ADDRESS_LABEL_MONSTER_PREFAB, (loadedRes) =>
        {
            if (loadedRes == null) return;
            // Debug.Log("Loaded : " + loadedRes.name);
            dictMonsterNameToPrefab.Add(loadedRes.name, loadedRes);
        });
    }

    public void InitializeEffectDict()
    {
        if (dictEffectNameToPrefab != null) 
        {
            Debug.LogWarning("Effect dict already initialized!");
            return;
        }

        dictEffectNameToPrefab = new Dictionary<string, GameObject>();
        Addressables.LoadAssetsAsync<GameObject>(Constants.ADDRESS_LABEL_EFFECT_PREFAB, (loadedRes) =>
        {
            if (loadedRes == null) return;
            MMFeedbacks feedbacks = loadedRes.GetComponent<MMFeedbacks>();

            if (feedbacks == null) 
            {
                Debug.Log(loadedRes.name + " has no feedback attached ! ");
                return;
            }

            dictEffectNameToPrefab.Add(loadedRes.name.ToLower(), loadedRes);
        });
    }
}
