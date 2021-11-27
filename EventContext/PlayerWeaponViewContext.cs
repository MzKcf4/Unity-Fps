using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponViewContext : MonoBehaviour
{
    public static PlayerWeaponViewContext Instance;
    
    public UnityEvent<WeaponEvent> onWeaponEventUpdate = new UnityEvent<WeaponEvent>();
    
    void Awake()
    {
        Instance = this;
    }
    
    public void EmitWeaponEvent(WeaponEvent newEvent)
    {
        onWeaponEventUpdate.Invoke(newEvent);
    }
}
