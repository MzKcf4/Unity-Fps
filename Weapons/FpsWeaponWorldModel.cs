using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MoreMountains.Feedbacks;
using Org.BouncyCastle.Asn1.Cmp;

// Represents the weapon dropped on world , or hold by character
// Should attach together with FpsWeapon.

public class FpsWeaponWorldModel : MonoBehaviour
{
    private MMFeedbacks muzzleFeedbacks;
    public Transform muzzleTransform;
    public GameObject bulletPrefab;

    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private Rigidbody rb;
    private BoxCollider collider;

    // Stores the information of the weapon dropped
    private FpsWeapon fpsWeapon;

    void Awake()
    {
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }
    
    void Start()
    {
        GameObject muzzleFeedbackPrefab = WeaponAssetManager.Instance.weaponMuzzleFeedbackPrefab;
        if (muzzleTransform != null)
        {
            GameObject muzzleFeedbackObj = Instantiate(muzzleFeedbackPrefab, muzzleTransform);
            muzzleFeedbackObj.transform.localPosition = Vector3.zero;
            muzzleFeedbacks = muzzleFeedbackObj.GetComponent<MMFeedbacks>();
        }

        /*
        if (skinnedMeshRenderers != null)
        { 
            var smr = skinnedMeshRenderers[0];
            var objWithRenderer = skinnedMeshRenderers[0].gameObject;

            collider = objWithRenderer.AddComponent<BoxCollider>();
            rb = objWithRenderer.AddComponent<Rigidbody>();
            
            rb.isKinematic = true;
            rb.useGravity = false;
            collider.isTrigger = true;

            // Get the renderer's world-space bounds
            Bounds worldBounds = smr.bounds;

            // Set the collider's center and size based on the world-space bounds
            collider.center = objWithRenderer.transform.InverseTransformPoint(worldBounds.center);
        }
        */
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

    private void OnEnable()
    {
        if (muzzleFeedbacks)
            muzzleFeedbacks.StopFeedbacks();
    }
}
