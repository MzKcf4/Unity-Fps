using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageConfig : ScriptableObject
{
	public List<GameObject> spawnPrefabList;
	public int maxSpawnCount;
}
