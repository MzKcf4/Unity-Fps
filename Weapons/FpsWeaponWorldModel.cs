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
        
    }
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject muzzleFeedbackPrefab = WeaponAssetManager.Instance.weaponMuzzleFeedbackPrefab;
        if (muzzleTransform != null)
        {
            GameObject muzzleFeedbackObj = Instantiate(muzzleFeedbackPrefab, muzzleTransform);
            muzzleFeedbackObj.transform.localPosition = Vector3.zero;
            muzzleFeedbacks = muzzleFeedbackObj.GetComponent<MMFeedbacks>();
        }
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
        if(muzzleFeedbacks && muzzleTransform)
            muzzleFeedbacks.PlayFeedbacks();
    }
}
