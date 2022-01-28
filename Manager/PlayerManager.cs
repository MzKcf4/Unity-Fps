using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
	public static PlayerManager Instance;
    
    private List<Transform> teamASpawns = new List<Transform>();
    private List<Transform> teamBSpawns = new List<Transform>();
    	
    [SerializeField] GameObject botPrefab;
    [SerializeField] GameObject botPrefab_B;
    
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
    public void AddBot(TeamEnum team, int skillLevel)
    {
        Transform spawn = GetSpawnTransform(team);
        
        GameObject botObj;
        if(team == TeamEnum.Red)
            botObj = Instantiate(botPrefab_B , spawn.position , Quaternion.identity);
        else
            botObj = Instantiate(botPrefab , spawn.position , Quaternion.identity);
        NetworkServer.Spawn(botObj);
        
        FpsBot fpsBot = botObj.GetComponent<FpsBot>();
        fpsBot.team = team;
        fpsBot.SetSkillLevel(skillLevel);
        
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
        Transform spawn = GetSpawnTransform(TeamEnum.Blue);
        player.TargetDoTeleport(player.netIdentity.connectionToClient, spawn.position + transform.up);
	}

    [Server]
    public void TeleportCharacterToSpawnPoint(FpsCharacter character)
    {
        if (character is FpsPlayer)
            TeleportToSpawnPoint((FpsPlayer)character);
        else
        {
            Transform spawn = GetSpawnTransform(character.team);
            character.transform.position = spawn.position + transform.up;
        }
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

        SetSpawnProtection(fpsCharacter);
    }

    [Server]
    public void SetSpawnProtection(FpsEntity fpsEntity)
    {
        fpsEntity.SetGodmode(true);
        StartCoroutine(RemoveGodModeCoroutine(fpsEntity));
    }

    [Server]
    public void QueueRespawn(FpsCharacter fpsCharacter)
    {
        StartCoroutine(RespawnCoroutine(fpsCharacter));
    }
    
    IEnumerator RespawnCoroutine(FpsCharacter fpsCharacter)
    {
        yield return new WaitForSeconds(5);
        
        if(fpsCharacter != null)
            RespawnAndTeleport(fpsCharacter);
    }

    IEnumerator RemoveGodModeCoroutine(FpsEntity fpsEntity)
    {
        yield return new WaitForSeconds(3);

        if (fpsEntity != null)
            fpsEntity.SetGodmode(false);
    }

    private Transform GetSpawnTransform(TeamEnum team)
    {
        if (teamASpawns.Count == 0 || teamBSpawns.Count == 0)
        {
            FindSpawnsOnMap();
        }

        if(team == TeamEnum.Blue)
            return Utils.GetRandomElement<Transform>(teamASpawns);
        else
            return Utils.GetRandomElement<Transform>(teamBSpawns);
    }

    private void FindSpawnsOnMap()
    {
        teamASpawns = new List<Transform>();
        teamBSpawns = new List<Transform>();

        GameObject[] teamASpawnObjects = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_A_SPAWN);

        foreach (GameObject obj in teamASpawnObjects)
            teamASpawns.Add(obj.transform);


        GameObject[] teamBSpawnObjects = GameObject.FindGameObjectsWithTag(Constants.TAG_TEAM_B_SPAWN);

        foreach (GameObject obj in teamBSpawnObjects)
            teamBSpawns.Add(obj.transform);
        
    }
}
