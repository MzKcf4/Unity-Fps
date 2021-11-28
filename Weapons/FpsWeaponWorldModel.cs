using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;

// Represents the weapon dropped on world , or hold by character
// Should attach together with FpsWeapon.

public class FpsWeaponWorldModel : MonoBehaviour
{
    [SerializeField] private WeaponResources weaponResources;
    private MMFeedbacks muzzleFeedbacks;
    private AudioSource audioSource;
    public Transform muzzleTransform;
    public GameObject bulletPrefab;
    
    
    void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
        muzzleFeedbacks = GetComponentInChildren<MMFeedbacks>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ShootProjectile()
    {
        ShootProjectile(transform.forward);
    }
    
    public void ShootProjectile(Vector3 dest)
    {
        if(muzzleFeedbacks)
            muzzleFeedbacks.PlayFeedbacks();
        
        
        PlayWeaponSound(Constants.WEAPON_SOUND_FIRE);
        /*
        Vector3 direction = Utils.GetDirection(muzzleTransform.position , dest);
        GameObject projectileObj = Instantiate(bulletPrefab , muzzleTransform.position , Quaternion.identity);
        NetworkServer.Spawn(projectileObj);
        FpsProjectile projectile = projectileObj.GetComponent<FpsProjectile>();
        projectile.speed = 120f;
        projectile.Setup(direction);
        */
    }
    
    public void PlayWeaponSound(string name)
    {
        name = name.ToLower();
        if(weaponResources == null || weaponResources.dictWeaponSounds == null)
            return;
        
        if(!weaponResources.dictWeaponSounds.ContainsKey(name))
            Debug.Log("Sound resource key not found : " + name);
        
        audioSource.PlayOneShot(weaponResources.dictWeaponSounds[name]);
    }
}
