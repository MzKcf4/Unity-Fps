using System.Linq;
using UnityEngine;

public class FpsWeaponView : MonoBehaviour
{    
    [SerializeField] private ArmBoneToWeaponBone armL4d;
    [SerializeField] private ArmBoneToWeaponBone armGmod;

    private ArmBoneToWeaponBone activeArm;

    public FpsWeaponViewModel[] weaponViewModelSlots = new FpsWeaponViewModel[Constants.WEAPON_SLOT_MAX];
    
    private FpsWeaponViewModel activeWeaponViewModel;
    
    void Awake()
    {
        armL4d = GetComponentsInChildren<ArmBoneToWeaponBone>().Where(bone => bone.isL4DArm).First();
        armGmod = GetComponentsInChildren<ArmBoneToWeaponBone>().Where(bone => !bone.isL4DArm).First();
        activeArm = armL4d;
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

        // Switch armL4d model
        if (activeWeaponViewModel.isFromL4D && activeArm != armL4d)
        {
            activeArm.gameObject.SetActive(false);
            activeArm = armL4d;
        }
        else if (!activeWeaponViewModel.isFromL4D && activeArm != armGmod)
        {
            activeArm.gameObject.SetActive(false);
            activeArm = armGmod;
        }
        activeArm.gameObject.SetActive(true);
        activeArm.AttachToWeapon(activeWeaponViewModel.gameObject);

        // Flip the "FpsView" object itself
        if (activeWeaponViewModel.isFlip)
            transform.localScale = new Vector3(-1f, 1f, 1f);
        else
            transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
