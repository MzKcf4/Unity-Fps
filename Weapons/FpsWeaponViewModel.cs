using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(Animator),typeof(AnimancerComponent))]
public class FpsWeaponViewModel : MonoBehaviour
{
    [SerializeField] public Vector3 offsetFromView = Vector3.zero;
    [SerializeField] WeaponResources weaponResources;
    private MMFeedbacks muzzleFeedbacks;
    private AnimancerComponent animancer;
    private AudioSource audioSource;
    
    void Awake()
    {
        animancer = GetComponent<AnimancerComponent>();
        muzzleFeedbacks = GetComponentInChildren<MMFeedbacks>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = AudioManager.Instance.localPlayerAudioSource;
        if (!muzzleFeedbacks)
        {
            ViewMuzzleMarker marker = GetComponentInChildren<ViewMuzzleMarker>();
            if (marker && WeaponAssetManager.Instance.weaponMuzzleFeedbackViewPrefab)
            {
                GameObject muzzleFeedbackObject = Instantiate(WeaponAssetManager.Instance.weaponMuzzleFeedbackViewPrefab, marker.transform);
                muzzleFeedbacks = muzzleFeedbackObject.GetComponent<MMFeedbacks>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetWeaponResources(WeaponResources weaponResources)
    {
        this.weaponResources = weaponResources;
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
        
        if(!weaponResources.dictWeaponSounds.ContainsKey(name))
            Debug.Log("Sound resource key not found : " + name);
        
        audioSource.PlayOneShot(weaponResources.dictWeaponSounds[name]);
    }
    
    public void AniEvent_PlayWeaponSound(string name)
    {
        PlayWeaponSound(name);
    }
}
