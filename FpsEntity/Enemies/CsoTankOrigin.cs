using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;

public class CsoTankOrigin : FpsNpc
{
	// ----------------
	[SerializeField]
	private ActionCooldown berserkCooldown;
	private float berserkRemaining = 0f;
	private bool isOnBerserk = false;
	// ----------------

	protected override void Update()
	{
		base.Update();
		CheckAndUseAbility();
	}
	
	public void CheckAndUseAbility()
	{
		if(!isServer)	return;
		
		if(berserkRemaining > 0)
			berserkRemaining -= Time.deltaTime;
		else
		{
			// Berserk ends
			if(isOnBerserk)
			{
				DoBerserkEnd();
			}

			if (IsDead()) return;

			berserkCooldown.ReduceCooldown(Time.deltaTime);
			if(!berserkCooldown.IsOnCooldown())
			{
				DoBerserk();
				berserkCooldown.StartCooldown();
			}
		}
	}
	
	protected void DoBerserk()
	{
		isOnBerserk = true;
		berserkRemaining = 8f;
		MultiplySpeed(1.5f);
		RpcDoBerserk();
	}
	
	protected void DoBerserkEnd()
	{
		isOnBerserk = false;
		MultiplySpeed((1f / 1.5f));
		RpcDoBerserkEnd();
	}
	
	[ClientRpc]
	protected void RpcDoBerserk()
	{
		EffectManager.Instance.SetColor(gameObject , Color.red);
	}
	
	[ClientRpc]
	protected void RpcDoBerserkEnd()
	{
		EffectManager.Instance.SetColor(gameObject , Color.white);
	}
}
