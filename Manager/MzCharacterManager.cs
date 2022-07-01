using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

// Holds and manages references to in-game MzCharacters
// Provides utilities methods for MzCharacters
public class MzCharacterManager : NetworkBehaviour
{
    public static MzCharacterManager instance;

    public UnityEvent<FpsCharacter> OnCharacterJoin { get { return characterJoinEvent; }}
    public UnityEvent<FpsCharacter> OnCharacterKilled { get { return characterKilledEvent; } }


    public UnityEvent<FpsCharacter> characterJoinEvent = new UnityEvent<FpsCharacter>();
    public UnityEvent<FpsCharacter> characterRemoveEvent = new UnityEvent<FpsCharacter>();
    public UnityEvent<FpsCharacter> characterSpawnEvent = new UnityEvent<FpsCharacter>();
    public UnityEvent<FpsCharacter> characterKilledEvent = new UnityEvent<FpsCharacter>();

    void Awake()
    {
        instance = this;
    }

    [Server]
    protected void RemoveCharacter(FpsCharacter character)
    {
        if(character != null)
            NetworkServer.Destroy(character.gameObject);
    }

    public void QueueRemove(FpsCharacter character , int seconds) 
    { 
        StartCoroutine(RemoveAfter(character , seconds));
    }

    IEnumerator RemoveAfter(FpsCharacter character , int seconds)
    { 
        yield return new WaitForSeconds(seconds);
        RemoveCharacter(character);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
