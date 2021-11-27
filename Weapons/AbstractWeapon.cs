/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit.Physic;
using MoreMountains.Feedbacks;
using Animancer;
using Cinemachine;
using Mirror;
*/
/*
public class AbstractWeapon : MonoBehaviour
{
	[SerializeField] private WeaponStaticResource weaponStaticResource;
	public C_WeaponStats weaponStats;
	protected AudioSource audioSource;
	
	public CinemachineImpulseSource cameraShake;
	
	private int clipSize = 30;
	private int ammo = 30;
	private int bpammo = 30;
	private bool isReload = false;
	
	protected bool canFire = true;
	private WeaponRecoil recoil;
	private bool isPrimaryFireByAnimation = false;
	private bool isPrimaryFire = false;
	private float nextPrimaryFire = 0f;
	
	private bool isSecondaryFire = false;
	
	[SerializeField] private Transform muzzleTransform;
	private MMFeedbacks primaryFireFeedbacks;
	public RaycastHelper raycastHelper;
	
	AnimancerComponent animancer;
	private bool isFrozen = false;
	
	void Awake()
	{
		weaponStats = C_WeaponStats.GetEntity(weaponStaticResource.weaponName);
		ammo = weaponStats.F_clip_size;
		isPrimaryFireByAnimation = weaponStats.F_primary_fire_by_animation;
		
		animancer = GetComponent<AnimancerComponent>();
		recoil = GetComponent<WeaponRecoil>();
		raycastHelper = Camera.main.GetComponent<RaycastHelper>();
		
		if(muzzleTransform != null)
		{
			GameObject muzzleFeedbackObj = Instantiate(ResourceManager.Instance.GetMuzzlePrefab(), muzzleTransform);
			primaryFireFeedbacks = muzzleFeedbackObj.GetComponent<MMFeedbacks>();
		}		
		
	}
	
	public void InitAfterAttach()
	{
		// Weapon audio is emitted from Hand's audioSource
		audioSource = GetComponentInParent<AudioSource>();
		cameraShake = GetComponentInParent<CinemachineImpulseSource>();
	}

    // Update is called once per frame
    void Update()
	{
		
		if(!isFrozen && isPrimaryFire && canFire && nextPrimaryFire <= 0)
		{
			animancer.Play(weaponStaticResource.primaryFireClip).Time = 0;
			nextPrimaryFire = weaponStats.F_primary_fire_interval;
			if(!isPrimaryFireByAnimation)
			{
				DoPrimaryFire();
			}
		}
		if(nextPrimaryFire > 0)
			nextPrimaryFire -= Time.deltaTime;
	}
	
	public void Unfreeze()
	{
		isFrozen = false;
	}
    
	public void Draw()
	{
		isFrozen = true;
		FpsUiManager.Instance.UpdateAmmo(ammo , bpammo);
		animancer.Play(weaponStaticResource.drawClip).Events.OnEnd = () => { 
			Unfreeze();
			animancer.Play(weaponStaticResource.idleClip, 0.1f, FadeMode.FromStart);
		};
	}
    
	public void UpdatePrimaryFire(bool isPrimaryFire)
	{
		this.isPrimaryFire = isPrimaryFire;
	}
	
	public void UpdateWeaponReload(bool isReload)
	{
		if(ammo == weaponStats.F_clip_size)
			return;
		
		if(!isFrozen)
		{
			isFrozen = true;
			animancer.Play(weaponStaticResource.reloadClip).Events.OnEnd = () => { 
				PostReload();
				animancer.Play(weaponStaticResource.idleClip, 0.1f, FadeMode.FromStart);
			};
		}
	}
	
	public void PostReload()
	{
		int bpammoNeeded = clipSize - ammo;
		bpammoNeeded = bpammoNeeded > bpammo ? bpammo : bpammoNeeded;
		bpammo -= bpammoNeeded;
		bpammo = bpammo >= 0 ? bpammo : 0;
		
		ammo = ammo + bpammoNeeded;
		FpsUiManager.Instance.UpdateAmmo(ammo , bpammo);
		Unfreeze();
	}
	
	// Usually called in AnimationClip
	public void DoPrimaryFire()
	{
		if(weaponStaticResource.primaryFireSound)
			audioSource.PlayOneShot(weaponStaticResource.primaryFireSound);
		
		if(primaryFireFeedbacks != null)
			primaryFireFeedbacks.PlayFeedbacks();
		
		ApplyRecoil();
		CheckRaycastHit();
		ammo--;
		FpsUiManager.Instance.UpdateAmmo(ammo , bpammo);
		Crosshair.Instance?.DoLerp();
	}
	
	private void CheckRaycastHit()
	{
		raycastHelper.CheckPhysic();		
		IEnumerable<RaycastHit> hits = raycastHelper.GetRaycastHits();
		foreach(RaycastHit hit in hits)
		{
			if(hit.collider.CompareTag("Enemy"))
			{
				FpsEntity enemy = hit.collider.GetComponent<FpsEntity>();
				if(enemy != null)
				{
					enemy.CmdProcessDamage(DamageInfo.AsDamageInfo(weaponStats.F_primary_fire_damage));
					SpawnManager.Instance.SpawnBloodFx(hit.point);
				}
			}
		}
	}
		
	private void ApplyRecoil()
	{
		if(cameraShake)
			cameraShake.GenerateImpulse(Camera.main.transform.forward);
	}
		
	public void PlaySound(string name)
	{
		if(audioSource == null || weaponStaticResource == null || string.IsNullOrEmpty(name))	return;
		AudioClip clip = weaponStaticResource.GetAudio(name);
		if(clip != null)
			audioSource.PlayOneShot(clip);
	}
}
*/