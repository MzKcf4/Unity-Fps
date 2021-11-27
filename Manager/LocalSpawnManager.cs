using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PathologicalGames;
using MoreMountains.Feedbacks;

public class LocalSpawnManager : MonoBehaviour
{
	public static LocalSpawnManager Instance;
	
	[SerializeField] private GameObject bloodFxPrefab;
    [SerializeField] private GameObject bulletDecalPrefab;
	
	void Awake()
	{
		Instance = this;
	}
	
	public void SpawnBloodFx(Vector3 position)
	{
		Transform objTransform = PoolManager.Pools["FX"].Spawn(bloodFxPrefab , position , Quaternion.identity);
		PoolManager.Pools["FX"].Despawn(objTransform , 2f);
	}
    
    public void SpawnBulletDecalFx(Vector3 position , Vector3 normalVector)
    {
        Transform objTransform = PoolManager.Pools["FX"].Spawn(bulletDecalPrefab , position , Quaternion.identity);
        objTransform.rotation = Quaternion.FromToRotation (objTransform.up,normalVector) * objTransform.rotation;
        
        MMFeedbacks feedbacks = objTransform.gameObject.GetComponent<MMFeedbacks>();
        
        if(feedbacks)
        {
            feedbacks.PlayFeedbacks();
        }
        
        
        PoolManager.Pools["FX"].Despawn(objTransform , 5f);
    }
}
