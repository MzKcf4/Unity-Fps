using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using BansheeGz.BGDatabase;

[CreateAssetMenu]
public class ProgressionWeaponConfig : SerializedScriptableObject
{
	public Dictionary<int , List<GameObject>> dictTierToWeaponPrefab = new Dictionary<int, List<GameObject>>();
	public Dictionary<string, GameObject> dictNameToWeaponPrefab;
		
	public void InitializeWeaponList()
	{
		dictTierToWeaponPrefab.Clear();
		
		foreach(KeyValuePair<string, GameObject> entry in dictNameToWeaponPrefab)
		{
			string weaponName = entry.Key;
			E_weapon_info weaponInfo = E_weapon_info.GetEntity(weaponName);
			if(weaponInfo == null)
			{
				Debug.LogWarning("Weapon : " + weaponName + " not registered in DB");
				continue;
			}
			int tier = weaponInfo.f_progression_tier;
			
			List<GameObject> tierWeaponList;
			if(!dictTierToWeaponPrefab.ContainsKey(tier))
			{
				tierWeaponList = new List<GameObject>();
				dictTierToWeaponPrefab.Add(tier , tierWeaponList);
			}
			else
			{
				tierWeaponList = dictTierToWeaponPrefab[tier];
			}
			tierWeaponList.Add(entry.Value);
		}
	}
}
