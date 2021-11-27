using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CsoSpeedHost : AbstractCsoEnemy
{
	[SerializeField]
	private ActionCooldown stalkCooldown;
	private float stalkRemaining = 0f;
	private bool isOnStalk = false;
	public float stalkDuration = 5f;
	
	[SerializeField]
	private SkinnedMeshRenderer renderer;
	[SerializeField]
	private Material normalMaterial;
	[SerializeField]
	private Material transparentMaterial;
	
	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
		CheckAndUseAbility();
	}
	
	public void CheckAndUseAbility()
	{
		if(!isServer)	return;
		
		if(stalkRemaining > 0)
			stalkRemaining -= Time.deltaTime;
		else
		{
			// Berserk ends
			if(isOnStalk)
			{
				DoStalkEnd();
			}
			
			stalkCooldown.ReduceCooldown(Time.deltaTime);
			if(!stalkCooldown.IsOnCooldown())
			{
				DoStalk();
				stalkCooldown.StartCooldown();
			}
		}
	}
	
	protected void DoStalk()
	{
		isOnStalk = true;
		stalkRemaining = stalkDuration;
		RpcDoStalk();
	}
	
	protected void DoStalkEnd()
	{
		isOnStalk = false;
		RpcDoStalkEnd();
	}
	
	[ClientRpc]
	protected void RpcDoStalk()
	{
		// material is additive for better transparent effect.
		renderer.material = transparentMaterial;
	}
	
	[ClientRpc]
	protected void RpcDoStalkEnd()
	{
		renderer.material = normalMaterial;
		EffectManager.Instance.SetColor(gameObject , Color.white);
	}
}
