using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
	public static PlayerManager Instance;
    
    public List<Transform> teamASpawns = new List<Transform>();
    public List<Transform> teamBSpawns = new List<Transform>();
    	
    [SerializeField] GameObject botPrefab;
    
	void Awake()
	{
		Instance = this;
	}
	
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
    [Server]
    public void AddBot(TeamEnum team)
    {
        Transform spawn = GetSpawnTransform(team);
        
        GameObject botObj = Instantiate(botPrefab , spawn.position , Quaternion.identity);
        NetworkServer.Spawn(botObj);
        
        FpsBot fpsBot = botObj.GetComponent<FpsBot>();
        fpsBot.team = team;
        
        RespawnAndTeleport(fpsBot);
    }
    
    [Server]
    public void KickAllBot()
    {
        List<FpsCharacter> charListClone =  new List<FpsCharacter>(SharedContext.Instance.characterList);
        
        foreach(FpsCharacter fpsCharacter in charListClone)
        {
            if(fpsCharacter is FpsBot)
            {
                NetworkServer.Destroy(fpsCharacter.gameObject);
            }
        }
    }
    
    
	[Server]
	public void TeleportToSpawnPoint(FpsPlayer player)
	{
		Transform spawn = Utils.GetRandomElement<Transform>(teamASpawns);
        player.TargetDoTeleport(player.netIdentity.connectionToClient, spawn.position + transform.up);
	}
    
        
    [Server]
    public void RespawnAndTeleport(FpsCharacter fpsCharacter)
    {
        Transform spawn = GetSpawnTransform(fpsCharacter.team);
        Vector3 spawnPos = spawn.position + transform.up;
        
        fpsCharacter.Respawn();
        if(fpsCharacter is FpsPlayer)
            ((FpsPlayer)fpsCharacter).TargetDoTeleport(fpsCharacter.netIdentity.connectionToClient, spawnPos);
        else
        {
            fpsCharacter.transform.position = spawnPos;
        }
            
    }
    
    [Server]
    public void QueueRespawn(FpsCharacter fpsCharacter)
    {
        StartCoroutine(RespawnCoroutine(fpsCharacter));
    }
    
    IEnumerator RespawnCoroutine(FpsCharacter fpsCharacter)
    {
        yield return new WaitForSeconds(5);
        
        RespawnAndTeleport(fpsCharacter);
    }
    
    private Transform GetSpawnTransform(TeamEnum team)
    {
        if(team == TeamEnum.TeamA)
            return Utils.GetRandomElement<Transform>(teamASpawns);
        else
            return Utils.GetRandomElement<Transform>(teamBSpawns);
    }
}
