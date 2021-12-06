using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using MoreMountains.Feedbacks;

public class FpsWeaponView : MonoBehaviour
{
    private AnimancerComponent animancer;
    private MMFeedbacks muzzleFeedbacks;
    
    [SerializeField] private ArmBoneToWeaponBone arm;
    public FpsWeaponViewModel[] weaponViewModelSlots = new FpsWeaponViewModel[Constants.WEAPON_SLOT_MAX];
    
    private FpsWeaponViewModel activeWeaponViewModel;
    
    void Awake()
    {
        arm = GetComponentInChildren<ArmBoneToWeaponBone>();
        PlayerWeaponViewContext.Instance.onWeaponEventUpdate.AddListener(OnWeaponEventUpdate);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnWeaponEventUpdate(WeaponEvent evt)
    {
        if(!this.isActiveAndEnabled) return;
        
        if(activeWeaponViewModel == null)
        {
            Debug.LogWarning("Received weapon event, but active weapon view is null !");
            return;
        }
        activeWeaponViewModel.HandleWeaponEvent(evt);
    }
    
    public void AddViewWeaponNew(GameObject weaponViewPrefab, int slot)
    {
        GameObject weaponViewObj = Instantiate(weaponViewPrefab , transform);
        FpsWeaponViewModel fpsWeaponViewModel = weaponViewObj.GetComponent<FpsWeaponViewModel>();
        fpsWeaponViewModel.transform.localPosition = fpsWeaponViewModel.offsetFromView;
        fpsWeaponViewModel.gameObject.SetActive(false);
        weaponViewModelSlots[slot] = fpsWeaponViewModel;
    }
    
    public void SwitchWeapon(int slot)
    {
        if(activeWeaponViewModel != null)
        {
            activeWeaponViewModel.gameObject.SetActive(false);
        }
        
        activeWeaponViewModel = weaponViewModelSlots[slot];
        activeWeaponViewModel.gameObject.SetActive(true);
        arm.AttachToWeapon(activeWeaponViewModel.gameObject);
    }
}
