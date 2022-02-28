using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Animancer;

public class ValveShockTrooper : FpsEnemy
{
	protected const int LAYER_BASE = 0;
	protected const int LAYER_UPPER_BODY = 1;
	
	[SerializeField]
	private NpcResources charRes;
	
	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		
		if(charRes == null)
			Debug.LogError("Character resource not found !");
		
		animancer.Layers[LAYER_UPPER_BODY].SetMask(charRes.upperBodyMask);
		animancer.Play(charRes.runClip);
	}
	
	// Update is called once per frame
	protected override void Update()
	{
		base.Update();
	}
	
	[ClientRpc]
	protected override void RpcDoRangedAttack(Vector3 direction)
	{
		base.RpcDoRangedAttack(direction);
		PlayRangedAnimation();
	}
	
	[ClientRpc]
	protected override void RpcDoMeleeAttack()
	{
		base.RpcDoMeleeAttack();
		// PlayAttackAnimation();
	}
	
	[ClientRpc]
	protected override void RpcTakeDamage(DamageInfo damageInfo)
	{
		base.RpcTakeDamage(damageInfo);
		audioSource.PlayOneShot(charRes.hurtAudio);
	}
	
	[ClientRpc]
	protected override void RpcKilled(DamageInfo damageInfo)
	{
		base.RpcKilled(damageInfo);
		ClipTransition deathClip = Utils.GetRandomElement<ClipTransition>(charRes.deathClips);
		animancer.Play(deathClip, 0f , FadeMode.FromStart);
		
		if(charRes.deathAudio != null)
			audioSource.PlayOneShot(charRes.deathAudio);
	}
	
	private void PlayAttackAnimation()
	{
		//var upperState = animancer.Layers[LAYER_UPPER_BODY].Play(charRes.meleeClip);
		//upperState.Events.OnEnd = () => { animancer.Layers[LAYER_UPPER_BODY].Play(charRes.idleClipUpper);};
	}
	
	private void PlayRangedAnimation()
	{
		//var upperState = animancer.Layers[LAYER_UPPER_BODY].Play(charRes.rangedClip);
		//upperState.Events.OnEnd = () => { animancer.Layers[LAYER_UPPER_BODY].Play(charRes.idleClipUpper);};
	}
}
