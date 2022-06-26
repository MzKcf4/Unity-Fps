using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class MonsterStageConfig : ScriptableObject
{
	public List<GameObject> spawnPrefabList;
	public List<SpawnSetting> spawnSettingList;
	// Dict<GameObject , float>  monsterWeight
	public int maxSpawnCount;
	public int targetKillCount;

	[Serializable]
	public class SpawnSetting
	{
		public GameObject spawnPrefab;
		public float weight;
	}
}
