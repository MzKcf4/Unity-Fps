using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class ServerContext : NetworkBehaviour
{
	public static ServerContext Instance;
    public UnityEvent<FpsCharacter, DamageInfo> characterKilledEventServer = new UnityEvent<FpsCharacter, DamageInfo>();
	public List<FpsPlayer> playerList = new List<FpsPlayer>();
    
    public List<FpsCharacter> characterList = new List<FpsCharacter>();
	
	void Awake()
	{
		Instance = this;
	}
    
    public void UpdateCharacterKilledEvent(FpsCharacter character, DamageInfo dmgInfo)
    {
        characterKilledEventServer.Invoke(character,dmgInfo);
    }
	
	public FpsPlayer GetRandomPlayer()
	{
		if(playerList.Count == 0)
			return null;
			
		int idx = Random.Range(0 , playerList.Count);
		return playerList[idx];
	}
    	
	public void DestroyAfterSeconds(int seconds , GameObject gameObject)
	{
		StartCoroutine(DestroyAfterSecondsCoroutine(seconds, gameObject));
	}
	
	private IEnumerator DestroyAfterSecondsCoroutine(int seconds, GameObject gameObject)
	{
		yield return new WaitForSeconds(seconds);
		
		NetworkServer.Destroy(gameObject);
	}
}
