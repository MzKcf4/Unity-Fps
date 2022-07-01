using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit.Physic;

public class CsoBoomerOrigin : CsoBoomerHost
{
	/*
	[SerializeField]
	private RaycastHelper radiationRaycast;
	
	public ActionCooldown radiationDamageCooldown;
	
	protected override void Update()
	{
		base.Update();
		CheckRadiationDamage(Time.deltaTime);
	}
	
	private void CheckRadiationDamage(float deltaTime)
	{
		if(IsDead())	return;
		
		if(radiationDamageCooldown.IsOnCooldown())
			radiationDamageCooldown.ReduceCooldown(deltaTime);
		else
		{
			radiationDamageCooldown.StartCooldown();
			// Check hit
			radiationRaycast.CheckPhysic();
			IEnumerable<Collider> colliders = radiationRaycast.GetOverlapColliders();
			foreach(Collider c in colliders)
			{
				if(c.CompareTag(Constants.TAG_PLAYER))
				{
					FpsPlayer fpsPlayer = c.GetComponent<FpsPlayer>();
					fpsPlayer.OnHitInLocalClient(this.gameObject);
				}
			}
		}
	}
	*/
}
