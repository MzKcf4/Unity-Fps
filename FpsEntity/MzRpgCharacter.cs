using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit.Physic;
using Pathfinding;
using Mirror;

// RpgCharacter doesn't have fps weapon slots
// Instead they use raycast for attacks.
// Usually should be controlled by bots
public class MzRpgCharacter : FpsCharacter
{
	public RaycastHelper MeleeRangeDetector { get { return meleeRangeDetector; } }

	protected RaycastHelper meleeRangeDetector;
	// private MzEcmAStarCharacter ecmAStarCharacter;
	private AIBase ai;

    protected override void Awake()
    {
        base.Awake();
		ai = GetComponent<AIBase>();
	}

    protected override void Start()
    {
        base.Start();
		InitializeMeleeRangeDetector();
		// ecmAStarCharacter = (MzEcmAStarCharacter)ecmCharacter;
		
	}

	// Usually triggered by animation events played in Client side.
	public virtual void CheckMeleeHit()
	{
		// ToDo: if it's only checked in client , then only need to return local player instead of all other players
		List<FpsPlayer> playerList = GetTargetsInMeleeRange();
		foreach(FpsPlayer player in playerList)
		{
			player.OnHitInLocalClient(this.gameObject);
		}
	}

	public List<FpsPlayer> GetTargetsInMeleeRange()
	{
		List<FpsPlayer> targets = new List<FpsPlayer>();
		meleeRangeDetector.CheckPhysic();
		IEnumerable<Collider> colliders = meleeRangeDetector.GetOverlapColliders();
		foreach (Collider c in colliders)
		{
			if (c.CompareTag(Constants.TAG_PLAYER))
			{
				FpsPlayer fpsPlayer = c.GetComponent<FpsPlayer>();
				if (fpsPlayer == null)
					fpsPlayer = c.GetComponentInParent<FpsPlayer>();

				if(fpsPlayer != null)
					targets.Add(fpsPlayer);

				
			}
		}
		return targets;
	}

    public override void SetMaxSpeed(float maxSpeed)
    {
		base.SetMaxSpeed(maxSpeed);
		ai.maxSpeed = maxSpeed;
	}

	[Server]
	public void ServerExtendMeleeRange(FpsCharacter targetCharcter)
	{
		RpcExtendMeleeRange(targetCharcter.netIdentity);
	}

	[ClientRpc]
	public void RpcExtendMeleeRange(NetworkIdentity playerIdentity)
	{
		float distance = Vector3.Distance(transform.position , playerIdentity.transform.position);
		float middlePoint = Mathf.Max(1 , distance / 2);
		meleeRangeDetector.m_LocalPosition = new Vector3(0, 1f, middlePoint);
		meleeRangeDetector.m_HalfExtends = new Vector3(1f, 1f, middlePoint);
	}
	

    protected void InitializeMeleeRangeDetector()
	{
		meleeRangeDetector = gameObject.AddComponent<RaycastHelper>();
		meleeRangeDetector.m_FixedUpdate = false;
		meleeRangeDetector.RayType = RaycastHelper.eRayType.BoxOverlap;
		meleeRangeDetector.m_LocalPosition = new Vector3(0, 1f, 1f);
		meleeRangeDetector.m_HalfExtends = new Vector3(1f, 1f, 1f);
		meleeRangeDetector.SetMemorySize(15);
		meleeRangeDetector.m_LayerMask = LayerMask.GetMask(Constants.LAYER_CHARACTER_MODEL, Constants.LAYER_LOCAL_PLAYER_MODEL);
	}
}
