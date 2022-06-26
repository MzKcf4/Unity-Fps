using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Animancer;
using Mirror;
using Kit.Physic;

// 1. Attach the FpsModel to the character
// 2. Attach AnimationEvent handler to the FpsModel gameobject object
// 3. FpsNpc should call FpsModel to execute animations for animation-dependent logics
// 4. AnimationEvent handler should call FpsNpc to execute related logics

public abstract partial class FpsNpc : FpsCharacter
{
	[SerializeField] protected NpcResources npcResources;
	protected IAstarAI ai;
	protected AnimancerComponent animancer;
	protected AudioSource audioSource;
	protected AIDestinationSetter aiDest;

	public ActionCooldown switchTargetCooldown = new ActionCooldown()
	{
		interval = 3f
	};
	public Transform target;
	public float moveSpeed = 2f;
	public float speedRecoverDuration = 0.5f;
	public float speedRecoverElapsed = 0f;

	protected bool canMelee = true;
	protected RaycastHelper meleeRangeDetector;
	public ActionCooldown meleeCooldown;
	public int meleeDamage;

	public bool canRanged;
	[SerializeField]
	// protected Sensor rangedDector;
	public ActionCooldown rangedCooldown;
	[SerializeField]
	private Transform projectileSpawn;
	[SerializeField]
	private GameObject projectilePrefab;

	protected override void Start()
	{
		base.Start();
		canRagdoll = false;
		ai = GetComponent<IAstarAI>();
		aiDest = GetComponent<AIDestinationSetter>();
		animancer = GetComponent<AnimancerComponent>();
		audioSource = GetComponent<AudioSource>();
		if (ai != null)
			ai.maxSpeed = moveSpeed;

		canMelee = npcResources.canMelee;
		if (canMelee)
		{
			meleeRangeDetector = GetComponentInChildren<RaycastHelper>();
			if (meleeRangeDetector)
			{
				meleeRangeDetector.m_LayerMask = LayerMask.GetMask(Constants.LAYER_CHARACTER_MODEL , Constants.LAYER_LOCAL_PLAYER_MODEL);
			}
		}
		team = TeamEnum.Monster;

		E_monster_info dbMonsterInfo = E_monster_info.GetEntity(npcResources.id);
		if (dbMonsterInfo != null)
		{
			ai.maxSpeed = dbMonsterInfo.f_move_speed;
			this.characterName = dbMonsterInfo.f_display_name;
			this.health = dbMonsterInfo.f_base_health;
		}
	}

    protected override void AttachModel()
    {
		modelObject = Instantiate(npcResources.modelPrefab , transform);
		modelObjectParent = modelObject.transform.parent.gameObject;

		fpsModel = modelObject.GetComponent<FpsModel>();
		// modelAnimancer = modelObject.GetComponent<AnimancerComponent>();
		characterAnimator.SetAttachedModel(fpsModel);
	}

    // Update is called once per frame
    protected override void Update()
	{
		base.Update();

		if (IsDead()) return;

		if (isServer)
		{
			UpdateTarget();
			AttackTargetInMeleeRange();
			// ShootTargetInRange();
			CooldownActions();
			RecoverSpeed(Time.deltaTime);
		}
	}

	private void RecoverSpeed(float deltaTime)
	{
		if (ai == null || ai.maxSpeed >= moveSpeed) return;

		if (speedRecoverElapsed < speedRecoverDuration)
		{
			ai.maxSpeed = Mathf.Lerp(0, moveSpeed, speedRecoverElapsed / speedRecoverDuration);
			speedRecoverElapsed += deltaTime;
		}
		else
		{
			ai.maxSpeed = moveSpeed;
		}
	}

	[Server]
	public void MultiplySpeed(float multiply)
	{
		moveSpeed *= multiply;

		if (ai != null)
			ai.maxSpeed = moveSpeed;
	}

	protected virtual void CooldownActions()
	{
		if (!isServer) return;
		meleeCooldown.ReduceCooldown(Time.deltaTime);
		switchTargetCooldown.ReduceCooldown(Time.deltaTime);
		rangedCooldown.ReduceCooldown(Time.deltaTime);
	}

	private void UpdateTarget()
	{
		if (!isServer || switchTargetCooldown.IsOnCooldown()) return;
		switchTargetCooldown.StartCooldown();
		FpsPlayer targetPlayer = null;

		if (target == null)
			targetPlayer = ServerContext.Instance.GetRandomPlayer();
		else
		{
			// Chance to switch to cloest player.
			if (Random.RandomRange(0, 100) < 25)
			{
				float shortestDist = float.MaxValue;
				foreach (FpsPlayer player in ServerContext.Instance.playerList)
				{
					float dist = Vector3.Distance(transform.position, player.transform.position);
					if (dist < shortestDist)
					{
						targetPlayer = player;
						shortestDist = dist;
					}
				}
			}
		}


		if (targetPlayer != null)
		{
			target = targetPlayer.transform;
			aiDest.target = target;
		}
	}

	/*
	[Server]
	protected void ShootTargetInRange()
	{
		if (!isServer || !canRanged || rangedDector == null || rangedCooldown.IsOnCooldown()) return;

		List<FpsPlayer> detectedPlayers = rangedDector.GetDetectedByComponent<FpsPlayer>();
		if (detectedPlayers != null && detectedPlayers.Count > 0)
		{
			DoRangedAttack(detectedPlayers[0].gameObject);
		}
		rangedCooldown.StartCooldown();
	}

	protected virtual void DoRangedAttack(GameObject target)
	{
		if (!isServer) return;

		Vector3 direction = (target.transform.position - transform.position).normalized;
		GameObject projectileObj = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.identity);
		NetworkServer.Spawn(projectileObj);
		FpsProjectile projectile = projectileObj.GetComponent<FpsProjectile>();
		projectile.Setup(direction);
		RpcDoRangedAttack(direction);
	}
	*/

	[ClientRpc]
	protected virtual void RpcDoRangedAttack(Vector3 direction)
	{
		// Usually plays the attack animation
	}


	[Server]
	protected void AttackTargetInMeleeRange()
	{
		if (!isServer || !canMelee || meleeRangeDetector == null || meleeCooldown.IsOnCooldown()) return;
		meleeRangeDetector.CheckPhysic();
		IEnumerable<Collider> colliders = meleeRangeDetector.GetOverlapColliders();
		foreach (Collider c in colliders)
		{
			if (c.CompareTag(Constants.TAG_PLAYER))
			{
				DoMeleeAttack();
				meleeCooldown.StartCooldown();
				break;
			}
		}
	}

	protected virtual void DoMeleeAttack()
	{
		if (!isServer) return;

		RpcDoMeleeAttack();
	}

	[ClientRpc]
	protected virtual void RpcDoMeleeAttack()
	{
		// Usually plays the melee attack animation
		// Then the check hit event will be fired in Client side.
		// PlayActionAnimation(npcResources.meleeClip, UPPER_LAYER , npcResources.idleClip);
	}

	protected virtual void CheckMeleeHit()
	{
		// Usually triggered by animation events played in Client side.
		meleeRangeDetector.CheckPhysic();
		IEnumerable<Collider> colliders = meleeRangeDetector.GetOverlapColliders();
		foreach (Collider c in colliders)
		{
			if (c.CompareTag(Constants.TAG_PLAYER))
			{
				FpsPlayer fpsPlayer = c.GetComponent<FpsPlayer>();
				if (fpsPlayer == null)
					fpsPlayer = c.GetComponentInParent<FpsPlayer>();

				fpsPlayer.OnHitInClient(this.gameObject);
			}
		}
	}

	[Server]
	public override void TakeDamage(DamageInfo damageInfo)
	{
		base.TakeDamage(damageInfo);
		if (ai != null)
		{
			ai.maxSpeed = 0f;
			speedRecoverElapsed = 0f;
		}

	}

	[ClientRpc]
	protected override void RpcTakeDamage(DamageInfo damageInfo)
	{
		LocalSpawnManager.Instance.SpawnBloodFx(damageInfo.hitPoint);
	}

	[Server]
	protected override void Killed()
	{
		base.Killed();
		AIBase aiComponent = GetComponent<AIBase>();
		if (aiComponent != null)
			aiComponent.enabled = false;

		// DisableAllColliders();
		// ServerContext.Instance.DestroyAfterSeconds(5, gameObject);
	}

	[ClientRpc]
	protected override void RpcKilled(DamageInfo damageInfo)
	{
		base.RpcKilled(damageInfo);
		// PlayFullBodyAnimation(Utils.GetRandomElement<ClipTransition>(npcResources.deathClips));
	}

	/*
	private void DisableAllColliders()
	{
		Collider collider = GetComponent<Collider>();
		if (collider != null)
			collider.enabled = false;

		Collider[] colliders = GetComponentsInChildren<Collider>();
		if (colliders != null)
		{
			foreach (Collider c in colliders)
			{
				c.enabled = false;
			}
		}
	}
	*/
	public override Vector3 GetMovementVelocity()
	{
		if (isServer)
		{
			if (ai == null)
				currentVelocity = Vector3.zero;
			else
				currentVelocity = ai.velocity;
		}

		return currentVelocity;
	}

	public void AniEvent_CheckMeleeHit()
	{
		CheckMeleeHit();
	}

}
