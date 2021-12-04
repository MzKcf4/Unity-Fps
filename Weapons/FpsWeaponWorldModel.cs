using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;

// Represents the weapon dropped on world , or hold by character
// Should attach together with FpsWeapon.

public class FpsWeaponWorldModel : MonoBehaviour
{
    private MMFeedbacks muzzleFeedbacks;
    public Transform muzzleTransform;
    public GameObject bulletPrefab;
    
    void Awake()
    {
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
    }
}
