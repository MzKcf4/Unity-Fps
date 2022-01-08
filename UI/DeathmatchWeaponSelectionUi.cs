using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

public class DeathmatchWeaponSelectionUi : MonoBehaviour
{
    public static DeathmatchWeaponSelectionUi Instance;
    
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform weaponSelectionPanelTransform;
    
    
    private List<GameObject> weaponButtonList = new List<GameObject>();
    
    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        LocalPlayerContext.Instance.onTempDeathmatchWeaponMenuToggleEvent.AddListener(this.OnWeaponSelectionPanelToggle);
        
        E_weapon_info.ForEachEntity(weaponInfo => {
            if(weaponInfo.f_active){
                AddWeaponButton(weaponInfo);
            }
        });
    }
    
    private void AddWeaponButton(E_weapon_info weaponInfo)
    {
        GameObject buttonObj = Instantiate(weaponButtonPrefab,weaponSelectionPanelTransform);
        ButtonManagerBasic button = buttonObj.GetComponent<ButtonManagerBasic>();
            
        string formattedText = weaponInfo.f_name + " ( " + weaponInfo.f_dm_kill_score + " )";
        button.buttonText = formattedText;
        
            
        button.clickEvent.AddListener(() => {
            LocalPlayerContext.Instance.StoreAdditionalValue(Constants.ADDITIONAL_KEY_DM_SELECTED_WEAPON, weaponInfo.f_name);
        });
            
        button.UpdateUI();
    }
    
    private void OnWeaponSelectionPanelToggle()
    {
        bool newState = !weaponSelectionPanelTransform.gameObject.activeSelf;
        weaponSelectionPanelTransform.gameObject.SetActive(newState);
        LocalPlayerContext.Instance.TogglePlayerControl(!newState);
    }
    
}
