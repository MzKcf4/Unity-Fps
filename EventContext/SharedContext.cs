using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class SharedContext : NetworkBehaviour
{
    public static SharedContext Instance;
    public List<FpsCharacter> characterList = new List<FpsCharacter>();
    public UnityEvent<FpsCharacter> characterJoinEvent = new UnityEvent<FpsCharacter>();
    public UnityEvent<FpsCharacter> characterRemoveEvent = new UnityEvent<FpsCharacter>();
    
    void Awake()
    {
        Instance = this;
    }
        
    public void RegisterCharacter(FpsCharacter fpsCharacter)
    {
        if(!characterList.Contains(fpsCharacter))
        {
            characterList.Add(fpsCharacter);
            characterJoinEvent.Invoke(fpsCharacter);
        }
    }
    
    public void RemoveCharacter(FpsCharacter fpsCharacter)
    {
        if(characterList.Contains(fpsCharacter))
        {
            characterList.Remove(fpsCharacter);
            characterRemoveEvent.Invoke(fpsCharacter);
        }
    }
    
    [ClientRpc]
    public void RpcRemoveCharacter(FpsCharacter fpsCharacter)
    {
        RemoveCharacter(fpsCharacter);
    }
    
}
