using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpawnManager : NetworkBehaviour
{
	public static SpawnManager Instance;
	public List<GameObject> prefabToSpawn;
	public List<Transform> positionToSpawn;
	
	[SerializeField]
	public ActionCooldown spawnInterval;
	public int spawnRemaining;
	
	void Awake()
	{
		Instance = this;
	}
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	    spawnInterval.ReduceCooldown(Time.deltaTime);
	    if(!spawnInterval.IsOnCooldown() && spawnRemaining > 0)
	    {
	    	DoSpawn();
	    	spawnRemaining--;
	    	spawnInterval.StartCooldown();
	    }
    }
    
	public void StartSpawn()
	{
		spawnRemaining = 10;
	}
    
	private void DoSpawn()
	{
		GameObject prefab = prefabToSpawn[0];
		
		GameObject obj = Instantiate(prefab, positionToSpawn[0].position , Quaternion.identity);
		NetworkServer.Spawn(obj);
	}
}
