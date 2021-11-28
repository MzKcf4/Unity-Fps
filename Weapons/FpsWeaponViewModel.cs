using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using MoreMountains.Feedbacks;

public class FpsWeaponViewModel : MonoBehaviour
{
    [SerializeField] public Vector3 offsetFromView = Vector3.zero;
    [SerializeField] WeaponResources weaponResources;
    private MMFeedbacks muzzleFeedbacks;
    private AnimancerComponent animancer;
    // Use parent's audio source
    public AudioSource audioSource;
    
    void Awake()
    {
        animancer = GetComponent<AnimancerComponent>();
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
