using UnityEngine;

public class FpsWeaponView : MonoBehaviour
{    
    [SerializeField] private ArmBoneToWeaponBone arm;
    public FpsWeaponViewModel[] weaponViewModelSlots = new FpsWeaponViewModel[Constants.WEAPON_SLOT_MAX];
    
    private FpsWeaponViewModel activeWeaponViewModel;
    
    void Awake()
    {
        arm = GetComponentInChildren<ArmBoneToWeaponBone>();
        LocalPlayerContext.Instance.onWeaponEventUpdate.AddListener(OnWeaponEventUpdate);
        
    }
    
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
    
    public void AddViewWeaponNew(WeaponResources weaponResources, int slot)
    {
        if (weaponViewModelSlots[slot] != null)
        {
            Destroy(weaponViewModelSlots[slot].gameObject, 0.5f);
        }
        GameObject weaponViewObj = Instantiate(weaponResources.weaponViewPrefab, transform);
        FpsWeaponViewModel fpsWeaponViewModel = weaponViewObj.GetComponent<FpsWeaponViewModel>();
        fpsWeaponViewModel.SetUp(weaponResources);
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

        // Flip the "FpsView" object itself
        if (activeWeaponViewModel.isFlip)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
