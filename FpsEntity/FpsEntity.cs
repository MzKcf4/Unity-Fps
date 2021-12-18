﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class FpsEntity : NetworkBehaviour
{
	[SerializeField] protected List<Behaviour> localPlayerBehaviours;
	[SerializeField] protected List<GameObject> localPlayerGameObjects;
	
	[SerializeField] protected List<Behaviour> serverBehaviours;
	[SerializeField] protected List<GameObject> serverGameObjects;
	
	// For enemies , it's the base health at stage 1 whem player count is 1
	[SyncVar] public int health = 100;
	[SyncVar] public int maxHealth = 100;
    
    [SyncVar] public bool isGodMode = false;
	
	public UnityEvent<GameObject> onKilledEvent = new UnityEvent<GameObject>();
	
    protected virtual void Awake()
    {
        
    }
	
    // Start is called before the first frame update
	protected virtual void Start()
    {
        SetupComponentsByNetworkSetting();
    }

    // Update is called once per frame
	protected virtual void Update()
    {
        
    }
    
    protected void SetupComponentsByNetworkSetting()
    {
        if(!isLocalPlayer)
        {
            foreach(Behaviour b in localPlayerBehaviours)
                b.enabled = false;
            foreach(GameObject o in localPlayerGameObjects)
                o.SetActive(false);
        }
        
        if(!isServer)
        {
            foreach(Behaviour b in serverBehaviours)
                b.enabled = false;
            foreach(GameObject o in serverGameObjects)
                o.SetActive(false);
        }
    }
    
	public bool IsDead()
	{
		return health <= 0;
	}
    
    [Server]
    public virtual void SetHealth(int newHealth)
    {
        health = newHealth;
        RpcHealthUpdate();
    }
    
    [ClientRpc]
    public virtual void RpcHealthUpdate()
    {
        
    }
	    
	[Server]
	public virtual void TakeDamage(DamageInfo damageInfo)
	{
		if(!isServer || IsDead() || isGodMode)	return;
		
        ProcessDamageByDistance(damageInfo);
		ProcessDamageByBodyPart(damageInfo);
		int newHealth = health - damageInfo.damage;
        SetHealth(newHealth);
		if(IsDead())
        {
            RpcTakeDamage(damageInfo);
			Killed(damageInfo);
        }
		else
        {
			RpcTakeDamage(damageInfo);
        }
	}
    
    private void ProcessDamageByDistance(DamageInfo damageInfo)
    {
        damageInfo.damage = Utils.CalculateDamageByDropoff(damageInfo.damage , damageInfo.GetDamageDistance() , damageInfo.weaponRangeModifier);
    }
	
	private void ProcessDamageByBodyPart(DamageInfo damageInfo)
	{
		if(damageInfo.bodyPart == BodyPart.Leg)
			damageInfo.damage = Mathf.RoundToInt(damageInfo.damage * 0.7f);
		else if (damageInfo.bodyPart == BodyPart.Head)
			damageInfo.damage *= 2;
	}
	
	[ClientRpc]
	protected virtual void RpcTakeDamage(DamageInfo damageInfo)
	{
		
	}
	
	[Server]
	public virtual void Kill()
	{
        Kill(new DamageInfo());
	}
    
    [Server]
    public virtual void Kill(DamageInfo damageInfo)
    {
        SetHealth(0);
        Killed(damageInfo);
    }

    [Server]
    protected virtual void Killed()
    {
        Killed(new DamageInfo());
    }

    [Server]
    protected virtual void Killed(DamageInfo damageInfo)
    {
        RpcKilled(damageInfo);
        onKilledEvent.Invoke(gameObject);
    }
	    
    [ClientRpc]
    protected virtual void RpcKilled(DamageInfo damageInfo)
    {
        
    }
}
