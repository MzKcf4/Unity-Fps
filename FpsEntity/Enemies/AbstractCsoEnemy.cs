using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Mirror;

public class AbstractCsoEnemy : FpsEnemy
{
	protected const int LAYER_BASE = 0;
	protected const int LAYER_UPPER_BODY = 1;
	
	[SerializeField]
	private CsoEnemyResources charRes;
	
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
	protected override void RpcDoMeleeAttack()
	{
		base.RpcDoMeleeAttack();
		PlayAttackAnimation();
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
		audioSource.PlayOneShot(charRes.deathAudio);
	}
	
	private void PlayAttackAnimation()
	{
		var upperState = animancer.Layers[LAYER_UPPER_BODY].Play(charRes.meleeClip);
		upperState.Events.OnEnd = () => { animancer.Layers[LAYER_UPPER_BODY].Play(charRes.idleClipUpper);};
	}
}
