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
    public Dictionary<string, AudioClip> dictAudioNameToAudioClip;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildWeaponResourceDict();
        InitializeAudioSourceDict();
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

    public AudioClip GetAudioClip(string clipName)
    {
        if (!dictAudioNameToAudioClip.ContainsKey(clipName)) 
        {
            Debug.LogWarning("Addressable Audio clip not found " + clipName);
            return null;
        }
        return dictAudioNameToAudioClip[clipName];
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
            dictEffectNameToPrefab.Add(loadedRes.name.ToLower(), loadedRes);
        });
    }

    public void InitializeAudioSourceDict()
    {
        if (dictAudioNameToAudioClip != null)
        {
            Debug.LogWarning("AudioSource dict already initialized!");
            return;
        }

        dictAudioNameToAudioClip = new Dictionary<string, AudioClip>();
        Addressables.LoadAssetsAsync<AudioClip>(Constants.ADDRESS_LABEL_AUDIO_RESOURCE, (loadedRes) =>
        {
            if (loadedRes == null) return;
            dictAudioNameToAudioClip.Add(loadedRes.name.ToLower(), loadedRes);
        });
    }
}
