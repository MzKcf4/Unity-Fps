using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
	public static PlayerManager Instance;
    
    public List<Transform> teamASpawns = new List<Transform>();
    public List<Transform> teamBSpawns = new List<Transform>();
	
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
        
    }
    
	[Server]
	public void TeleportToSpawnPoint(FpsPlayer player)
	{
        
		Transform spawn = Utils.GetRandomElement<Transform>(teamASpawns);
		player.transform.position = spawn.position + transform.up;
	}
    
    [Server]
    public void QueueRespawn(FpsCharacter fpsCharacter)
    {
        StartCoroutine(RespawnCoroutine(fpsCharacter));
    }
    
    IEnumerator RespawnCoroutine(FpsCharacter fpsCharacter)
    {

        yield return new WaitForSeconds(5);
        
        Transform spawn;
        if(fpsCharacter.team == TeamEnum.TeamA)
            spawn = Utils.GetRandomElement<Transform>(teamASpawns);
        else
            spawn = Utils.GetRandomElement<Transform>(teamBSpawns);
        
        fpsCharacter.Respawn();
        fpsCharacter.transform.position = spawn.position + transform.up;
    }
}
