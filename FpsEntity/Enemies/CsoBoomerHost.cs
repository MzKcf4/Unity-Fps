using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Mirror;
using Kit.Physic;

public class CsoBoomerHost : FpsNpc
{
	[SerializeField] 
	private MMFeedbacks explosionFeedbacks;
	[SerializeField]
	private RaycastHelper explosionRaycast;
	
	[ClientRpc]
	protected override void RpcKilled(DamageInfo damageInfo)
	{
		base.RpcKilled(damageInfo);
		explosionFeedbacks.PlayFeedbacks();
		
		// Check hit
		explosionRaycast.CheckPhysic();
		IEnumerable<Collider> colliders = explosionRaycast.GetOverlapColliders();
		foreach(Collider c in colliders)
		{
			if(c.CompareTag(Constants.TAG_PLAYER))
			{
				FpsPlayer fpsPlayer = c.GetComponent<FpsPlayer>();
				fpsPlayer.OnHitInClient(this.gameObject);
			}
		}
	}
	
	
}
