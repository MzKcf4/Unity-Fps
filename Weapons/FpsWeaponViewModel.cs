using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Animator),typeof(AnimancerComponent))]
public class FpsWeaponViewModel : MonoBehaviour
{
    private Vector3 offsetFromView = Vector3.zero;
    private WeaponResources weaponResources;
    private MMFeedbacks muzzleFeedbacks;
    private AnimancerComponent animancer;
    private AudioSource audioSource;
    [HideInInspector] public bool isFlip = false;

    void Awake()
    {
        animancer = GetComponent<AnimancerComponent>();
        muzzleFeedbacks = GetComponentInChildren<MMFeedbacks>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = LocalPlayerContext.Instance.localPlayerAudioSource;
        AttachMuzzleFeedback();
    }

    private void AttachMuzzleFeedback()
    {
        if (muzzleFeedbacks || !WeaponAssetManager.Instance.weaponMuzzleFeedbackViewPrefab)
            return;

        ViewMuzzleMarker marker = GetComponentInChildren<ViewMuzzleMarker>();
        if (marker)
        {
            GameObject muzzleFeedbackObject = Instantiate(WeaponAssetManager.Instance.weaponMuzzleFeedbackViewPrefab, marker.transform);
            muzzleFeedbacks = muzzleFeedbackObject.GetComponent<MMFeedbacks>();
        }
        else
        {
            Transform flashTransform = transform.FirstOrDefault(x => {
                return x.name.Contains("flash") 
                        // INS2
                        || x.name.Contains("A_Muzzle");
            });

            if (flashTransform != null) {
                GameObject muzzleFeedbackObject = Instantiate(WeaponAssetManager.Instance.weaponMuzzleFeedbackViewPrefab, flashTransform);
                muzzleFeedbacks = muzzleFeedbackObject.GetComponent<MMFeedbacks>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetUp(WeaponResources weaponResources)
    {
        this.weaponResources = weaponResources;
        string weaponId = weaponResources.weaponId;

        E_weapon_view_info dbViewInfo = E_weapon_view_info.FindEntity(e =>
            string.Equals(weaponResources.weaponId, e.f_weapon_info.f_name)
        );

        if (dbViewInfo != null)
        {
            offsetFromView = dbViewInfo.f_view_offset;
            isFlip = dbViewInfo.f_is_flip;
        }
        else
            offsetFromView = Vector3.zero;

        transform.localPosition = offsetFromView;
    }
    
    public void HandleWeaponEvent(WeaponEvent evt)
    {
        if(evt == WeaponEvent.Draw)
            animancer.Play(weaponResources.drawClip , 0f , FadeMode.FromStart);
        else if (evt == WeaponEvent.Reload)
            animancer.Play(weaponResources.reloadClip , 0f , FadeMode.FromStart);
        else if (evt == WeaponEvent.Shoot)
        {
            animancer.Play(weaponResources.shootClip , 0f , FadeMode.FromStart);
            if(muzzleFeedbacks != null)
                muzzleFeedbacks.PlayFeedbacks();
        }
        else if (evt == WeaponEvent.Reload_PalletStart)
            animancer.Play(weaponResources.palletReload_StartClip , 0f , FadeMode.FromStart);
        else if (evt == WeaponEvent.Reload_PalletInsertStart)
            animancer.Play(weaponResources.palletReload_InsertClip , 0f , FadeMode.FromStart);
        else if (evt == WeaponEvent.Reload_PalletEnd)
            animancer.Play(weaponResources.palletReload_EndClip , 0f , FadeMode.FromStart);
    }
    
    public void PlayWeaponSound(string name)
    {
        if(weaponResources == null || weaponResources.dictWeaponSounds == null)
            return;

        if (!weaponResources.dictWeaponSounds.ContainsKey(name))
        {
            Debug.Log("Sound resource key not found : " + name);
            return;
        }

        audioSource.PlayOneShot(weaponResources.dictWeaponSounds[name]);
    }
    
    public void AniEvent_PlayWeaponSound(string name)
    {
        PlayWeaponSound(name);
    }

    private void OnEnable()
    {
        if (muzzleFeedbacks)
            muzzleFeedbacks.StopFeedbacks();
    }
}
